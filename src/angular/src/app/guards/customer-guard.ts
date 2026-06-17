import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth';

export const customerGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // If user is logged in as an admin, block access to customer routes and redirect to admin dashboard
  if (authService.getToken() && authService.isAdmin) {
    return router.parseUrl('/admin');
  }

  // Otherwise, allow access
  return true;
};
