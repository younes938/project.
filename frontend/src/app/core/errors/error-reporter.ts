import { Injectable, signal } from '@angular/core';
import { ApplicationError, toApplicationError } from './application-error';

@Injectable({ providedIn: 'root' })
export class ErrorReporter {
  private readonly currentErrorState = signal<ApplicationError | null>(null);

  readonly currentError = this.currentErrorState.asReadonly();

  report(error: unknown): void {
    this.currentErrorState.set(toApplicationError(error));
  }

  clear(): void {
    this.currentErrorState.set(null);
  }
}
