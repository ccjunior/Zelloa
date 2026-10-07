import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { FamilyAuthenticationService } from './family-authentication.service';

export const guardianGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(FamilyAuthenticationService);
  const router = inject(Router);

  try {
    const user = await auth.loadCurrentUser();
    return user.roles.includes('Guardian')
      ? true
      : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  } catch {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
};
