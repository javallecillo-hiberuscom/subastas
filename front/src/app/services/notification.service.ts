import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';
import { getApiUrl } from '../utils/api-url.helper';

export interface Notificacion {
  id: number;
  tipo: 'puja_ganada' | 'puja_superada' | 'subasta_finalizada' | 'nueva_subasta' | 'registro';
  mensaje: string;
  fecha: Date;
  leida: boolean;
  idSubasta?: number;
  idVehiculo?: number;
  vehiculoInfo?: string; // Info adicional del vehículo para mostrar
  idUsuario?: number; // Para notificaciones admin
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  
  notificaciones = signal<Notificacion[]>([]);
  noLeidas = signal(0);
  
  private checkInterval: any;
  private esAdmin = false;

  iniciarMonitoreo(idUsuario: number, esAdmin: boolean = false) {
    console.error('🔔🔔🔔 NotificationService: INICIANDO MONITOREO - Usuario:', idUsuario, 'esAdmin:', esAdmin);
    this.esAdmin = esAdmin;
    // Cargar notificaciones iniciales
    this.cargarNotificaciones(idUsuario);
    
    // Verificar cada 3 segundos para notificaciones en tiempo real
    this.checkInterval = setInterval(() => {
      this.cargarNotificaciones(idUsuario);
    }, 3000);
  }

  detenerMonitoreo() {
    if (this.checkInterval) {
      clearInterval(this.checkInterval);
    }
  }

  private cargarNotificaciones(idUsuario: number) {
    // Si es admin, usar endpoint de notificaciones admin, si no, endpoint de usuario
    const url = this.esAdmin 
      ? getApiUrl('/api/NotificacionesAdmin?soloNoLeidas=false')
      : getApiUrl(`/api/Notificaciones/${idUsuario}`);
    
    this.http.get<any[]>(url)
      .pipe(
        catchError((error) => {
          if (error.status === 404) {
            return of([]);
          }
          console.error('Error cargando notificaciones:', error);
          return of([]);
        })
      )
      .subscribe({
        next: (notificaciones) => {
          const notifArray = Array.isArray(notificaciones) ? notificaciones : [];

          const mapped = notifArray.map((n) => {
            return {
              id: n.idNotificacion ?? n.id, // Soporta ambos nombres
              tipo: n.tipo,
              mensaje: n.mensaje,
              fecha: n.fecha ? new Date(n.fecha) : new Date(),
              leida: n.leida ?? false,
              idSubasta: n.idSubasta,
              idVehiculo: n.idVehiculo,
              vehiculoInfo: '',
              idUsuario: n.idUsuario
            };
          });

          // Log de depuración para ver el array mapeado
          console.warn('🔔 Array de notificaciones mapeado:', mapped);

          this.notificaciones.set(mapped);
          this.noLeidas.set(mapped.filter(n => !n.leida).length);
        }
      });
  }
  
  private extraerInfoVehiculo(mensaje: string): string {
    // Intentar extraer información del vehículo del mensaje
    // Por ejemplo: "Has sido superado en la puja de la subasta #1" -> "Subasta #1"
    const match = mensaje.match(/subasta #(\d+)/i);
    if (match) {
      return `Subasta #${match[1]}`;
    }
    return '';
  }

  private mapearTipo(tipo: string): any {
    if (!tipo) return 'nueva_subasta'; // Valor por defecto si tipo es undefined
    
    // Normalizar a minúsculas
    const tipoLower = tipo.toLowerCase();
    
    const mapa: any = {
      'pujaganada': 'puja_ganada',
      'puja_ganada': 'puja_ganada',
      'pujasuperada': 'puja_superada',
      'puja_superada': 'puja_superada',
      'subastafinalizada': 'subasta_finalizada',
      'subasta_finalizada': 'subasta_finalizada',
      'nuevasubasta': 'nueva_subasta',
      'nueva_subasta': 'nueva_subasta',
      'registro': 'registro',
      'documento_subido': 'registro',
      'puja': 'puja_ganada',
      'otro': 'nueva_subasta'
    };
    
    return mapa[tipoLower] || tipoLower;
  }

  marcarComoLeida(idNotificacion: number) {
    // Si es admin, usar endpoint de admin
    const url = this.esAdmin 
      ? getApiUrl(`/api/NotificacionesAdmin/${idNotificacion}/marcar-leida`)
      : getApiUrl(`/api/Notificaciones/${idNotificacion}/leida`);
    
    this.http.put(url, {}).pipe(
      catchError((error) => {
        // Endpoint no implementado aún, solo actualizar localmente sin mostrar error
        if (error.status !== 404) {
          console.error('Error marcando notificación:', error);
        }
        return of(null); // Devolver observable vacío para continuar el flujo
      })
    ).subscribe({
      next: () => {
        const notifs = this.notificaciones();
        const index = notifs.findIndex(n => n.id === idNotificacion);
        if (index !== -1) {
          notifs[index].leida = true;
          this.notificaciones.set([...notifs]);
          this.noLeidas.set(notifs.filter(n => !n.leida).length);
        }
      }
    });
  }

  marcarTodasComoLeidas(idUsuario: number) {
    // Marcar todas las notificaciones no leídas individualmente
    const noLeidas = this.notificaciones().filter(n => !n.leida);
    noLeidas.forEach(notif => {
      this.marcarComoLeida(notif.id);
    });
  }

  procesarSubastasFinalizadas() {
    return this.http.post(getApiUrl('/api/Notificaciones/procesar-finalizadas'), {});
  }

  agregarNotificacionLocal(notif: Omit<Notificacion, 'id' | 'fecha' | 'leida'>) {
    const nuevaNotif: Notificacion = {
      ...notif,
      id: Date.now(),
      fecha: new Date(),
      leida: false
    };
    
    const notifs = [nuevaNotif, ...this.notificaciones()];
    this.notificaciones.set(notifs);
    this.noLeidas.set(this.noLeidas() + 1);
    
    // Mostrar notificación del navegador si tiene permiso
    if ('Notification' in window && Notification.permission === 'granted') {
      new Notification('Desguaces Borox', {
        body: notif.mensaje,
        icon: '/favicon.ico'
      });
    }
  }

  solicitarPermiso() {
    if ('Notification' in window && Notification.permission === 'default') {
      Notification.requestPermission();
    }
  }
}
