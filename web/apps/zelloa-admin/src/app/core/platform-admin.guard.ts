import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { PlatformAuthenticationService } from './platform-authentication.service';

export const platformAdminGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(PlatformAuthenticationService);
  const router = inject(Router);

  try {
    const user = await auth.loadCurrentUser();
    if (user.tenantId === null && user.roles.length === 1 && user.roles[0] === 'PlatformAdmin') {
      return true;
    }

    await auth.clearRejectedSession();
    return router.createUrlTree(['/login'], { queryParams: { unauthorized: 'true' } });
  } catch {
    auth.user.set(null);
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
};
