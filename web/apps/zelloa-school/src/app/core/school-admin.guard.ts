import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { AuthenticationService } from './authentication.service';

export const schoolAdminGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthenticationService);
  const router = inject(Router);

  try {
    const user = await auth.loadCurrentUser();
    return user.roles.includes('SchoolAdmin')
      ? true
      : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  } catch {
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  }
};
