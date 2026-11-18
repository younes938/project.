import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { OperatingContextGateway } from '../../application/ports/operating-context.gateway';
import { AppConfig } from '../../core/config/app-config';
import {
  CheckpointOption,
  OperatingContextSelection,
  OperatingContextSummary,
} from '../../domain/models/operating-context';

export class ApiOperatingContextGateway implements OperatingContextGateway {
  private readonly contextUrl: string;
  private readonly antiforgeryUrl: string;

  constructor(
    private readonly http: HttpClient,
    config: AppConfig,
  ) {
    const apiBaseUrl = config.apiBaseUrl.replace(/\/$/, '');
    this.contextUrl = `${apiBaseUrl}/operating-context`;
    this.antiforgeryUrl = `${apiBaseUrl}/auth/antiforgery`;
  }

  getOptions(): Promise<readonly CheckpointOption[]> {
    return firstValueFrom(
      this.http.get<readonly CheckpointOption[]>(`${this.contextUrl}/options`, {
        withCredentials: true,
      }),
    );
  }

  getCurrent(): Promise<OperatingContextSummary | null> {
    return firstValueFrom(
      this.http.get<OperatingContextSummary | null>(`${this.contextUrl}/current`, {
        withCredentials: true,
      }),
    );
  }

  async select(selection: OperatingContextSelection): Promise<OperatingContextSummary> {
    await firstValueFrom(this.http.get<void>(this.antiforgeryUrl, { withCredentials: true }));

    return firstValueFrom(
      this.http.put<OperatingContextSummary>(`${this.contextUrl}/current`, selection, {
        withCredentials: true,
      }),
    );
  }
}
