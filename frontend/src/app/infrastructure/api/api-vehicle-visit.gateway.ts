import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { VehicleVisitGateway } from '../../application/ports/vehicle-visit.gateway';
import { AppConfig } from '../../core/config/app-config';
import {
  CreatedVehicleEntry,
  CreateVehicleEntry,
  QueueLookupResult,
  VehicleVisitSummary,
} from '../../domain/models/vehicle-visit';

export class ApiVehicleVisitGateway implements VehicleVisitGateway {
  private readonly visitUrl: string;
  private readonly entryUrl: string;
  private readonly antiforgeryUrl: string;

  constructor(
    private readonly http: HttpClient,
    config: AppConfig,
  ) {
    const apiBaseUrl = config.apiBaseUrl.replace(/\/$/, '');
    this.visitUrl = `${apiBaseUrl}/vehicle-visits`;
    this.entryUrl = `${apiBaseUrl}/vehicle-entry`;
    this.antiforgeryUrl = `${apiBaseUrl}/auth/antiforgery`;
  }

  list(): Promise<readonly VehicleVisitSummary[]> {
    return firstValueFrom(
      this.http.get<readonly VehicleVisitSummary[]>(this.visitUrl, { withCredentials: true }),
    );
  }

  getById(id: string): Promise<CreatedVehicleEntry | null> {
    return firstValueFrom(
      this.http.get<CreatedVehicleEntry | null>(`${this.entryUrl}/${encodeURIComponent(id)}`, {
        withCredentials: true,
      }),
    );
  }

  lookupQueue(plateNumber: string): Promise<QueueLookupResult> {
    return firstValueFrom(
      this.http.get<QueueLookupResult>(`${this.entryUrl}/equeue`, {
        params: { plateNumber },
        withCredentials: true,
      }),
    );
  }

  async createEntry(command: CreateVehicleEntry): Promise<CreatedVehicleEntry> {
    await firstValueFrom(this.http.get<void>(this.antiforgeryUrl, { withCredentials: true }));
    return firstValueFrom(
      this.http.post<CreatedVehicleEntry>(this.entryUrl, command, { withCredentials: true }),
    );
  }
}
