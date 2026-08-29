import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';

/// Permite accesul doar utilizatorilor autentificati; altfel redirect la login,
/// pastrand URL-ul curent pentru a reveni dupa autentificare.
export const authGuard: CanActivateFn = (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);

  return auth.isLoggedIn()
    ? true
    : router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
};
