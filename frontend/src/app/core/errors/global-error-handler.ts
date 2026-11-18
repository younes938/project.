import { ErrorHandler, Injectable, inject } from '@angular/core';
import { ErrorReporter } from './error-reporter';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly errorReporter = inject(ErrorReporter);

  handleError(error: unknown): void {
    this.errorReporter.report(error);
  }
}
