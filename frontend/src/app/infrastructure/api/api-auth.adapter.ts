import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { firstValueFrom, of, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthAdapter, AuthenticatedUserSummary } from '../../application/ports/external-adapters';
import { AppConfig } from '../../core/config/app-config';

export class ApiAuthAdapter implements AuthAdapter {
  private readonly authUrl: string;

  constructor(
    private readonly http: HttpClient,
    config: AppConfig,
  ) {
    this.authUrl = `${config.apiBaseUrl.replace(/\/$/, '')}/auth`;
  }

  getCurrentSession(): Promise<AuthenticatedUserSummary | null> {
    return firstValueFrom(
      this.http
        .get<AuthenticatedUserSummary>(`${this.authUrl}/session`, { withCredentials: true })
        .pipe(
          catchError((error: unknown) =>
            error instanceof HttpErrorResponse && error.status === 401
              ? of(null)
              : throwError(() => error),
          ),
        ),
    );
  }

  async signIn(): Promise<AuthenticatedUserSummary> {
    await this.issueAntiforgeryToken();
    return firstValueFrom(
      this.http.post<AuthenticatedUserSummary>(`${this.authUrl}/mock/sign-in`, null, {
        withCredentials: true,
      }),
    );
  }

  async signOut(): Promise<void> {
    await this.issueAntiforgeryToken();
    await firstValueFrom(
      this.http.post<void>(`${this.authUrl}/logout`, null, { withCredentials: true }),
    );
  }

  async recordActivity(): Promise<void> {
    await this.issueAntiforgeryToken();
    await firstValueFrom(
      this.http.post<void>(`${this.authUrl}/activity`, null, { withCredentials: true }),
    );
  }

  private async issueAntiforgeryToken(): Promise<void> {
    await firstValueFrom(
      this.http.get<void>(`${this.authUrl}/antiforgery`, { withCredentials: true }),
    );
  }
}
