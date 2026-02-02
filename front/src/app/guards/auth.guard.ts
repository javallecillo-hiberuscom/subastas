import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isAuth = authService.checkAuthentication();
  
  if (isAuth) {
    return true;
  }
  
  // Redirigir al login guardando la URL solicitada
  router.navigate(['/login'], {
    queryParams: { returnUrl: state.url }
  });
  return false;
};
