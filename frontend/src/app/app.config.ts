import {
  ApplicationConfig,
  ErrorHandler,
  inject,
  provideAppInitializer,
  provideBrowserGlobalErrorListeners,
} from '@angular/core';
import { provideRouter } from '@angular/router';

import { routes } from './app.routes';
import { APP_CONFIG, DEFAULT_APP_CONFIG } from './core/config/app-config';
import { GlobalErrorHandler } from './core/errors/global-error-handler';
import { AuthStore } from './core/auth/auth.store';
import { IdleSessionService } from './core/auth/idle-session.service';
import { provideInfrastructure } from './infrastructure/providers';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    { provide: APP_CONFIG, useValue: DEFAULT_APP_CONFIG },
    provideInfrastructure(),
    provideAppInitializer(() => {
      const auth = inject(AuthStore);
      inject(IdleSessionService).start();
      return auth.initialize();
    }),
    { provide: ErrorHandler, useClass: GlobalErrorHandler },
  ],
};
