import { Routes } from '@angular/router';
import { authGuard } from './guards/auth.guard';
import { adminGuard } from './guards/admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'registro',
    loadComponent: () => import('./registro/registro.component').then(m => m.RegistroComponent)
  },
  {
    path: '',
    canActivate: [authGuard],
    loadComponent: () => import('./layout/layout.component').then(m => m.LayoutComponent),
    children: [
      {
        path: '',
        redirectTo: '/dashboard',
        pathMatch: 'full'
      },
      {
        path: 'dashboard',
        loadComponent: () => import('./dashboard/dashboard.component').then(m => m.DashboardComponent)
      },
      {
        path: 'admin/dashboard',
        canActivate: [adminGuard],
        loadComponent: () => import('./dashboard-admin/dashboard-admin.component').then(m => m.DashboardAdminComponent)
      },
      {
        path: 'subastas',
        loadComponent: () => import('./lista-pujas/lista-pujas.component').then(m => m.ListaPujasComponent)
      },
      {
        path: 'subastas/:id',
        loadComponent: () => import('./detalle-vehiculo/detalle-vehiculo.component').then(m => m.DetalleVehiculoComponent)
      },
      {
        path: 'mis-pujas',
        loadComponent: () => import('./mis-pujas/mis-pujas.component').then(m => m.MisPujasComponent)
      },
      {
        path: 'perfil',
        loadComponent: () => import('./perfil/perfil.component').then(m => m.PerfilComponent)
      },
      {
        path: 'subir-iae',
        loadComponent: () => import('./subir-iae/subir-iae.component').then(m => m.SubirIaeComponent)
      },
      {
        path: 'admin/vehiculos',
        canActivate: [adminGuard],
        loadComponent: () => import('./admin/vehiculos/vehiculos-admin.component').then(m => m.VehiculosAdminComponent)
      },
      {
        path: 'admin/empresas',
        canActivate: [adminGuard],
        loadComponent: () => import('./admin/empresas/empresas-admin.component').then(m => m.EmpresasAdminComponent)
      },
      {
        path: 'admin/pujas',
        canActivate: [adminGuard],
        loadComponent: () => import('./admin/pujas/pujas-admin.component').then(m => m.PujasAdminComponent)
      },
      {
        path: 'admin/usuarios',
        canActivate: [adminGuard],
        loadComponent: () => import('./admin/usuarios/usuarios.component').then(m => m.UsuariosComponent)
      },
      {
        path: 'admin/notificaciones',
        canActivate: [adminGuard],
        loadComponent: () => import('./admin/notificaciones-admin/notificaciones-admin.component').then(m => m.NotificacionesAdminComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: '/login'
  }
];
