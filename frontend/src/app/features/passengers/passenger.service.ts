import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { firstValueFrom } from 'rxjs';

export type PassengerStatus = 'Allowed' | 'Denied' | 'Pending Verification';
export type TravelDirection = 'Entry' | 'Exit';

export interface PassengerRecord {
  id: string;
  fullName: string;
  passportNumber: string;
  nationality: string;
  originCountry: string;
  destinationCountry: string;
  status: PassengerStatus;
  direction: TravelDirection;
  notes: string;
  createdAt: string;
}

export type PassengerDraft = Omit<PassengerRecord, 'id' | 'createdAt'>;

@Injectable({ providedIn: 'root' })
export class PassengerService {
  private readonly http = inject(HttpClient);

  async list(): Promise<PassengerRecord[]> {
    return firstValueFrom(
      this.http.get<PassengerRecord[]>('/api/passengers', { withCredentials: true }),
    );
  }

  async create(draft: PassengerDraft): Promise<PassengerRecord> {
    await this.issueAntiforgeryToken();
    return firstValueFrom(
      this.http.post<PassengerRecord>('/api/passengers', draft, { withCredentials: true }),
    );
  }

  async update(id: string, draft: PassengerDraft): Promise<PassengerRecord> {
    await this.issueAntiforgeryToken();
    return firstValueFrom(
      this.http.put<PassengerRecord>(`/api/passengers/${id}`, draft, { withCredentials: true }),
    );
  }

  async remove(id: string): Promise<void> {
    await this.issueAntiforgeryToken();
    await firstValueFrom(
      this.http.delete<void>(`/api/passengers/${id}`, { withCredentials: true }),
    );
  }

  private async issueAntiforgeryToken(): Promise<void> {
    await firstValueFrom(
      this.http.get<void>('/api/auth/antiforgery', { withCredentials: true }),
    );
  }

}
