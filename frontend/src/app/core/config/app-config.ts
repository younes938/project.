import { InjectionToken } from '@angular/core';

export type AdapterMode = 'mock' | 'api';

export interface AppConfig {
  readonly adapterMode: AdapterMode;
  readonly apiBaseUrl: string;
}

export const APP_CONFIG = new InjectionToken<AppConfig>('APP_CONFIG');

export const DEFAULT_APP_CONFIG: AppConfig = Object.freeze({
  adapterMode: 'mock',
  apiBaseUrl: '/api',
});
