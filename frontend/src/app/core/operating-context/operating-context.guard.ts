import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { OperatingContextStore } from './operating-context.store';

export const operatingContextGuard: CanActivateFn = async (_route, state) => {
  const operatingContext = inject(OperatingContextStore);
  const router = inject(Router);

  await operatingContext.refresh();
  return operatingContext.hasContext()
    ? true
    : router.createUrlTree(['/operating-context'], {
        queryParams: { returnUrl: state.url },
      });
};
