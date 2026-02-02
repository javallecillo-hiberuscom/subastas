import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthService } from '../services/auth.service';

export const adminGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  // Primero verificar si está autenticado
  if (!authService.checkAuthentication()) {
    router.navigate(['/login'], {
      queryParams: { returnUrl: state.url }
    });
    return false;
  }

  // Luego verificar si es administrador
  const currentUser = authService.currentUser();
  const rolLower = currentUser?.rol?.toLowerCase() || '';
  if (rolLower === 'administrador' || rolLower === 'admin') {
    return true;
  }

  // Si no es admin, redirigir al dashboard
  router.navigate(['/dashboard']);
  return false;
};
