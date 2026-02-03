import { Component, inject, computed, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { ToastComponent } from '../components/toast/toast.component';

interface MenuItem {
  label: string;
  icon: string;
  route?: string;
  queryParams?: any;
  adminOnly?: boolean;
  userOnly?: boolean;
  submenu?: MenuItem[];
}

@Component({
  selector: 'app-layout',
  imports: [CommonModule, RouterModule, ToastComponent],
  templateUrl: './layout.component.html',
  styleUrl: './layout.component.css'
})
export class LayoutComponent implements OnInit, OnDestroy {
  private authService = inject(AuthService);
  private router = inject(Router);
  notificationService = inject(NotificationService);

  currentUser = this.authService.currentUser;
  sidebarOpen = true;
  showNotifications = false;
  expandedMenus = signal(new Set<string>());

  menuItems: MenuItem[] = [
    { label: 'Dashboard', icon: '🏠', route: '/dashboard', userOnly: true },
    { label: 'Dashboard Admin', icon: '📊', route: '/admin/dashboard', adminOnly: true },
    { 
      label: 'Vehículos en Subasta', 
      icon: '🚗',
      submenu: [
        { label: 'Subastas Activas', icon: '🔥', route: '/subastas', queryParams: { filtro: 'activas' } },
        { label: 'Subastas Vencidas', icon: '⏱️', route: '/subastas', queryParams: { filtro: 'vencidas' } }
      ]
    },
    { label: 'Mis Pujas', icon: '💰', route: '/mis-pujas', userOnly: true },
    { label: 'Mi Perfil', icon: '👤', route: '/perfil' },
    { label: 'Subir Documento IAE', icon: '📄', route: '/subir-iae', userOnly: true },
    { label: 'Notificaciones', icon: '🔔', route: '/admin/notificaciones', adminOnly: true },
    { label: 'Gestión Vehículos', icon: '🔧', route: '/admin/vehiculos', adminOnly: true },
    { label: 'Gestión Empresas', icon: '🏢', route: '/admin/empresas', adminOnly: true },
    { label: 'Gestión Pujas', icon: '📊', route: '/admin/pujas', adminOnly: true },
    { label: 'Gestión Usuarios', icon: '👥', route: '/admin/usuarios', adminOnly: true }
  ];

  visibleMenuItems = computed(() => {
    const user = this.currentUser();
    if (!user) return [];
    
    // Verificar múltiples variantes de administrador
    const rolLower = user.rol?.toLowerCase() || '';
    const isAdmin = rolLower === 'administrador' || rolLower === 'admin';
    
    return this.menuItems.filter(item => {
      if (item.adminOnly) return isAdmin;
      if (item.userOnly) return !isAdmin;
      return true;
    });
  });

  ngOnInit() {
    const user = this.currentUser();
    console.log('LayoutComponent: Usuario actual:', user);
    // Usar idUsuario que es la propiedad correcta, con fallback a id
    const userId = user?.idUsuario || (user?.id ? parseInt(user.id) : null);
    
    // Verificar si el usuario es administrador
    const rolLower = user?.rol?.toLowerCase() || '';
    const esAdmin = rolLower === 'administrador' || rolLower === 'admin';
    
    console.log('LayoutComponent: userId:', userId, 'rolLower:', rolLower, 'esAdmin:', esAdmin);
    
    if (userId) {
      console.log('LayoutComponent: Iniciando monitoreo de notificaciones para usuario:', userId, 'esAdmin:', esAdmin);
      this.notificationService.iniciarMonitoreo(userId, esAdmin);
      this.notificationService.solicitarPermiso();
    } else {
      console.warn('LayoutComponent: No se pudo obtener el ID del usuario para iniciar monitoreo de notificaciones', user);
    }
  }

  ngOnDestroy() {
    this.notificationService.detenerMonitoreo();
  }

  toggleSidebar(): void {
    this.sidebarOpen = !this.sidebarOpen;
  }

  toggleNotifications(): void {
    this.showNotifications = !this.showNotifications;
    if (this.showNotifications) {
      console.log('=== DEBUG NOTIFICACIONES ===');
      console.log('Total notificaciones:', this.notificationService.notificaciones().length);
      console.log('No leídas:', this.notificationService.noLeidas());
      console.log('Notificaciones:', this.notificationService.notificaciones());
    }
  }

  verNotificacion(notif: any): void {
    if (!notif.leida) {
      this.notificationService.marcarComoLeida(notif.id);
    }
    
    // Si es notificación de registro (admin), navegar a gestión de usuarios
    if (notif.tipo === 'registro' && notif.idUsuario) {
      this.router.navigate(['/admin/usuarios'], { 
        queryParams: { destacar: notif.idUsuario } 
      });
    }
    // Navegar a la subasta si existe
    else if (notif.idSubasta) {
      this.router.navigate(['/subastas', notif.idSubasta]);
    }
    
    this.showNotifications = false;
  }

  marcarTodasLeidas(): void {
    const user = this.currentUser();
    // Usar idUsuario con fallback a id
    const userId = user?.idUsuario || (user?.id ? parseInt(user.id) : null);
    if (userId) {
      this.notificationService.marcarTodasComoLeidas(userId);
    }
  }

  toggleSubmenu(label: string): void {
    const menus = new Set(this.expandedMenus());
    if (menus.has(label)) {
      menus.delete(label);
    } else {
      menus.add(label);
    }
    this.expandedMenus.set(menus);
  }

  getTiempoTranscurrido(fecha: Date | string): string {
    try {
      const ahora = new Date();
      const fechaNotif = typeof fecha === 'string' ? new Date(fecha) : fecha;
      
      // Verificar que la fecha es válida
      if (isNaN(fechaNotif.getTime())) {
        return 'Hace un momento';
      }
      
      const diff = ahora.getTime() - fechaNotif.getTime();
      const minutos = Math.floor(diff / 60000);
      
      if (minutos < 1) return 'Ahora';
      if (minutos < 60) return `Hace ${minutos}m`;
      
      const horas = Math.floor(minutos / 60);
      if (horas < 24) return `Hace ${horas}h`;
      
      const dias = Math.floor(horas / 24);
      return `Hace ${dias}d`;
    } catch (error) {
      console.error('Error calculando tiempo:', error);
      return 'Hace un momento';
    }
  }
  
  getMensajeNotificacion(notif: any): string {
    // Si tiene información del vehículo, construir mensaje descriptivo
    if (notif.vehiculoInfo && notif.vehiculoInfo !== '') {
      // Reemplazar referencias a "subasta #N" con información del vehículo
      let mensaje = notif.mensaje;
      
      // Si vehiculoInfo no es solo "Subasta #N", usarlo para reemplazar
      if (!notif.vehiculoInfo.startsWith('Subasta #')) {
        mensaje = mensaje.replace(/subasta #\d+/i, `${notif.vehiculoInfo}`);
        mensaje = mensaje.replace(/de la subasta/i, `del ${notif.vehiculoInfo}`);
      }
      
      return mensaje;
    }
    return notif.mensaje;
  }

  logout(): void {
    this.notificationService.detenerMonitoreo();
    this.authService.logout();
  }
}
