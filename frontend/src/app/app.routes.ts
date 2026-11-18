import { Routes } from '@angular/router';
import { anonymousOnlyGuard, authGuard, requirePermission } from './core/auth/auth.guards';
import { operatingContextGuard } from './core/operating-context/operating-context.guard';

export const routes: Routes = [
  {
    path: 'sign-in',
    canActivate: [anonymousOnlyGuard],
    loadComponent: () => import('./features/auth/sign-in/sign-in').then(({ SignIn }) => SignIn),
  },
  {
    path: 'forbidden',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/auth/access-denied/access-denied').then(
        ({ AccessDenied }) => AccessDenied,
      ),
  },
  {
    path: 'operating-context',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/operating-context/operating-context-selection').then(
        ({ OperatingContextSelection }) => OperatingContextSelection,
      ),
  },
  {
    path: 'passengers',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () =>
      import('./features/passengers/passengers').then(({ Passengers }) => Passengers),
  },
  {
    path: 'shipments',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () => import('./features/operations/operations-page').then(({ OperationsPage }) => OperationsPage),
    data: { mode: 'shipments' },
  },
  {
    path: 'inspections',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () => import('./features/operations/operations-page').then(({ OperationsPage }) => OperationsPage),
    data: { mode: 'inspections' },
  },
  {
    path: 'reports',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () => import('./features/operations/operations-page').then(({ OperationsPage }) => OperationsPage),
    data: { mode: 'reports' },
  },
  {
    path: 'audit',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () => import('./features/operations/operations-page').then(({ OperationsPage }) => OperationsPage),
    data: { mode: 'audit' },
  },
  {
    path: 'gate-devices',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () => import('./features/gate-devices/gate-devices').then(({ GateDevices }) => GateDevices),
  },
  {
    path: 'vehicle-visits',
    canActivate: [authGuard, operatingContextGuard, requirePermission('vehicle.view')],
    loadComponent: () =>
      import('./features/vehicle-visits/vehicle-visits').then(({ VehicleVisits }) => VehicleVisits),
  },
  {
    path: 'vehicle-entry',
    canActivate: [authGuard, operatingContextGuard, requirePermission('vehicle.process')],
    loadComponent: () =>
      import('./features/vehicle-entry/vehicle-entry').then(({ VehicleEntry }) => VehicleEntry),
  },
  {
    path: '',
    canActivate: [authGuard, operatingContextGuard],
    loadComponent: () =>
      import('./features/platform-status/platform-status').then(
        ({ PlatformStatus }) => PlatformStatus,
      ),
  },
  {
    path: '**',
    redirectTo: '',
  },
];
