import { HttpClient, provideHttpClient, withXsrfConfiguration } from '@angular/common/http';
import { EnvironmentProviders, makeEnvironmentProviders } from '@angular/core';
import {
  AUTH_ADAPTER,
  AuthAdapter,
  BORDER_CONTROL_ADAPTER,
  CUSTOMS_ADAPTER,
  EQUEUE_ADAPTER,
  VMS_ADAPTER,
} from '../application/ports/external-adapters';
import {
  VEHICLE_VISIT_GATEWAY,
  VehicleVisitGateway,
} from '../application/ports/vehicle-visit.gateway';
import {
  OPERATING_CONTEXT_GATEWAY,
  OperatingContextGateway,
} from '../application/ports/operating-context.gateway';
import { AppConfig, APP_CONFIG } from '../core/config/app-config';
import { ApiVehicleVisitGateway } from './api/api-vehicle-visit.gateway';
import { ApiAuthAdapter } from './api/api-auth.adapter';
import { ApiOperatingContextGateway } from './api/api-operating-context.gateway';
import {
  MockBorderControlAdapter,
  MockCustomsAdapter,
  MockEQueueAdapter,
  MockVmsAdapter,
} from './mock/mock-external-adapters';
import { MockVehicleVisitGateway } from './mock/mock-vehicle-visit.gateway';
import { MockOperatingContextGateway } from './mock/mock-operating-context.gateway';

function createVehicleVisitGateway(
  config: AppConfig,
  http: HttpClient,
  operatingContextGateway: OperatingContextGateway,
): VehicleVisitGateway {
  return config.adapterMode === 'api'
    ? new ApiVehicleVisitGateway(http, config)
    : new MockVehicleVisitGateway(operatingContextGateway);
}

function createAuthAdapter(config: AppConfig, http: HttpClient): AuthAdapter {
  // Authentication must use the ASP.NET Core session cookie because protected
  // endpoints (including Gate Monitoring and Weight History) require a server session.
  // Keep the other adapters controlled by adapterMode to avoid changing unrelated modules.
  void config;
  return new ApiAuthAdapter(http, config);
}

function createOperatingContextGateway(
  config: AppConfig,
  http: HttpClient,
): OperatingContextGateway {
  return config.adapterMode === 'api'
    ? new ApiOperatingContextGateway(http, config)
    : new MockOperatingContextGateway();
}

export function provideInfrastructure(): EnvironmentProviders {
  return makeEnvironmentProviders([
    provideHttpClient(
      withXsrfConfiguration({
        cookieName: 'XSRF-TOKEN',
        headerName: 'X-XSRF-TOKEN',
      }),
    ),
    {
      provide: VEHICLE_VISIT_GATEWAY,
      useFactory: createVehicleVisitGateway,
      deps: [APP_CONFIG, HttpClient, OPERATING_CONTEXT_GATEWAY],
    },
    {
      provide: AUTH_ADAPTER,
      useFactory: createAuthAdapter,
      deps: [APP_CONFIG, HttpClient],
    },
    {
      provide: OPERATING_CONTEXT_GATEWAY,
      useFactory: createOperatingContextGateway,
      deps: [APP_CONFIG, HttpClient],
    },
    { provide: VMS_ADAPTER, useClass: MockVmsAdapter },
    { provide: EQUEUE_ADAPTER, useClass: MockEQueueAdapter },
    { provide: CUSTOMS_ADAPTER, useClass: MockCustomsAdapter },
    { provide: BORDER_CONTROL_ADAPTER, useClass: MockBorderControlAdapter },
  ]);
}
