import { Component, inject, signal, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { ToastService } from '../../services/toast.service';
import { interval, Subscription } from 'rxjs';
import { getApiUrl } from '../../utils/api-url.helper';

interface NotificacionAdmin {
  id: number;
  idNotificacion?: number;
  titulo?: string;
  mensaje: string;
  tipo?: string;
  fecha?: Date;
  fechaEnvio?: Date;
  fechaCreacion?: Date;
  leida: boolean | number;
  idSubasta?: number;
  idVehiculo?: number;
  vehiculoInfo?: string;
  idUsuario?: number;
  datosAdicionales?: string;
  usuario?: {
    idUsuario: number;
    nombre: string;
    apellidos: string;
    email: string;
  };
}

@Component({
  selector: 'app-notificaciones-admin',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './notificaciones-admin.component.html',
  styleUrl: './notificaciones-admin.component.css'
})
export class NotificacionesAdminComponent implements OnInit, OnDestroy {
    notificacionEdit: NotificacionAdmin = {
      id: 0,
      titulo: '',
      mensaje: '',
      tipo: '',
      fechaCreacion: new Date(),
      leida: 0
    };
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private router = inject(Router);

  notificaciones = signal<NotificacionAdmin[]>([]);
  loading = signal(true);
  contadorNoLeidas = signal(0);
  filtro = signal<'todas' | 'no-leidas'>('no-leidas');
  
  private pollingSubscription?: Subscription;

  ngOnInit(): void {
    this.cargarNotificaciones();
    this.cargarContador();
    
    // Actualizar cada 30 segundos
    this.pollingSubscription = interval(30000).subscribe(() => {
      this.cargarNotificaciones();
      this.cargarContador();
    });
  }

  ngOnDestroy(): void {
    this.pollingSubscription?.unsubscribe();
  }

  cargarNotificaciones(): void {
    const soloNoLeidas = this.filtro() === 'no-leidas';
    this.loading.set(true);
    
    this.http.get<any[]>(getApiUrl(`/api/NotificacionesAdmin?soloNoLeidas=${soloNoLeidas}`))
      .subscribe({
        next: (notificaciones) => {
          // Mapear las propiedades a las esperadas por el frontend
          const mapped = (notificaciones || []).map(n => ({
            id: n.id ?? n.idNotificacion ?? n.IdNotificacion,
            idNotificacion: n.idNotificacion ?? n.id ?? n.IdNotificacion,
            titulo: n.titulo ?? n.Titulo ?? '',
            mensaje: n.mensaje ?? n.Mensaje ?? '',
            tipo: n.tipo ?? n.Tipo ?? '',
            idUsuario: n.idUsuario ?? n.IdUsuario,
            leida: n.leida ?? n.Leida ?? 0,
            fechaCreacion: n.fechaCreacion ?? n.fecha ?? n.FechaCreacion ?? '',
            usuario: n.usuario ?? n.Usuario ?? undefined
          }));
          this.notificaciones.set(mapped);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Error cargando notificaciones:', error);
          this.notificaciones.set([]);
          this.loading.set(false);
        }
      });
  }

  cargarContador(): void {
    this.http.get<{contador: number}>(getApiUrl('/api/NotificacionesAdmin/contador-no-leidas'))
      .subscribe({
        next: (response) => {
          this.contadorNoLeidas.set(response?.contador || 0);
        },
        error: (error) => {
          console.error('Error cargando contador:', error);
          this.contadorNoLeidas.set(0);
        }
      });
  }

  cambiarFiltro(filtro: 'todas' | 'no-leidas'): void {
    this.filtro.set(filtro);
    this.cargarNotificaciones();
  }

  marcarComoLeida(notificacion: NotificacionAdmin): void {
    if (notificacion.leida === 1) return;
    // Si es notificación de registro, navegar a gestión de usuarios
    if (notificacion.tipo === 'registro' && notificacion.idUsuario) {
      this.router.navigate(['/admin/usuarios'], { 
        queryParams: { destacar: notificacion.idUsuario } 
      });
    }
    this.http.put(getApiUrl(`/api/NotificacionesAdmin/${notificacion.idNotificacion}/marcar-leida`), {})
      .subscribe({
        next: () => {
          notificacion.leida = 1;
          this.cargarContador();
          this.toast.success('Notificación marcada como leída');
        },
        error: (error) => {
          console.error('Error marcando notificación:', error);
          this.toast.error('Error al marcar como leída');
        }
      });
  }

  marcarTodasLeidas(): void {
    if (this.contadorNoLeidas() === 0) {
      this.toast.info('No hay notificaciones sin leer');
      return;
    }

    this.http.put(getApiUrl('/api/NotificacionesAdmin/marcar-todas-leidas'), {})
      .subscribe({
        next: () => {
          this.cargarNotificaciones();
          this.cargarContador();
          this.toast.success('Todas las notificaciones marcadas como leídas');
        },
        error: (error) => {
          console.error('Error marcando todas:', error);
          this.toast.error('Error al marcar todas como leídas');
        }
      });
  }

  eliminarNotificacion(notificacion: NotificacionAdmin): void {
    if (!confirm('¿Eliminar esta notificación?')) return;

    this.http.delete(getApiUrl(`/api/NotificacionesAdmin/${notificacion.idNotificacion}`))
      .subscribe({
        next: () => {
          this.notificaciones.update(notifs => 
            notifs.filter(n => n.idNotificacion !== notificacion.idNotificacion)
          );
          this.cargarContador();
          this.toast.success('Notificación eliminada');
        },
        error: (error) => {
          console.error('Error eliminando notificación:', error);
          this.toast.error('Error al eliminar');
        }
      });
  }

  limpiarLeidas(): void {
    if (!confirm('¿Eliminar todas las notificaciones leídas?')) return;

    this.http.delete(getApiUrl('/api/NotificacionesAdmin/limpiar-leidas'))
      .subscribe({
        next: () => {
          this.cargarNotificaciones();
          this.toast.success('Notificaciones leídas eliminadas');
        },
        error: (error) => {
          console.error('Error limpiando:', error);
          this.toast.error('Error al limpiar notificaciones');
        }
      });
  }

  navegarAUsuario(idUsuario?: number): void {
    if (!idUsuario) return;
    this.router.navigate(['/admin/usuarios']);
  }

  getTipoIcono(tipo?: string): string {
    const iconos: { [key: string]: string } = {
      'registro': '👤',
      'documento_subido': '📄',
      'puja': '💰',
      'otro': 'ℹ️'
    };
    return iconos[tipo ?? ''] || 'ℹ️';
  }

  getTipoClase(tipo?: string): string {
    return `tipo-${tipo ?? ''}`;
  }

  getFormattedDate(fecha?: string | Date): string {
    if (!fecha) return '';
    const date = new Date(fecha);
    const now = new Date();
    const diff = now.getTime() - date.getTime();
    const minutos = Math.floor(diff / 60000);
    const horas = Math.floor(minutos / 60);
    const dias = Math.floor(horas / 24);

    if (minutos < 1) return 'Ahora';
    if (minutos < 60) return `Hace ${minutos}m`;
    if (horas < 24) return `Hace ${horas}h`;
    if (dias < 7) return `Hace ${dias}d`;
    
    return date.toLocaleDateString('es-ES', { 
      day: '2-digit', 
      month: 'short',
      year: 'numeric'
    });
  }

  guardarNotificacion(): void {
    // Implementar lógica de guardado aquí
    this.toast.info('Funcionalidad de guardar no implementada');
  }
}
