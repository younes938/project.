import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Permission } from '../../domain/permissions/permission';
import { AuthStore } from './auth.store';

export const authGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  await auth.initialize();

  return auth.isAuthenticated()
    ? true
    : router.createUrlTree(['/sign-in'], { queryParams: { returnUrl: state.url } });
};

export const anonymousOnlyGuard: CanActivateFn = async () => {
  const auth = inject(AuthStore);
  const router = inject(Router);
  await auth.initialize();

  return auth.isAuthenticated() ? router.createUrlTree(['/']) : true;
};

export function requirePermission(permission: Permission): CanActivateFn {
  return async () => {
    const auth = inject(AuthStore);
    const router = inject(Router);
    await auth.initialize();

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/sign-in']);
    }

    return auth.hasPermission(permission) ? true : router.createUrlTree(['/forbidden']);
  };
}

export function requireCheckpointAccess(parameterName = 'checkpointId'): CanActivateFn {
  return async (route) => {
    const auth = inject(AuthStore);
    const router = inject(Router);
    await auth.initialize();

    if (!auth.isAuthenticated()) {
      return router.createUrlTree(['/sign-in']);
    }

    const checkpointId = route.paramMap.get(parameterName);
    return checkpointId && auth.canAccessCheckpoint(checkpointId)
      ? true
      : router.createUrlTree(['/forbidden']);
  };
}
