import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/// Permite accesul doar adminilor; userii obisnuiti sunt trimisi la catalog.
export const adminGuard: CanActivateFn = () => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isAdmin() ? true : router.createUrlTree(['/']);
};
