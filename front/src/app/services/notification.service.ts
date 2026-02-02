import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { interval, of } from 'rxjs';
import { catchError, tap } from 'rxjs/operators';

export interface Notificacion {
  id: number;
  tipo: 'puja_ganada' | 'puja_superada' | 'subasta_finalizada' | 'nueva_subasta';
  mensaje: string;
  fecha: Date;
  leida: boolean;
  idSubasta?: number;
  idVehiculo?: number;
  vehiculoInfo?: string; // Info adicional del vehículo para mostrar
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  
  notificaciones = signal<Notificacion[]>([]);
  noLeidas = signal(0);
  
  private checkInterval: any;

  iniciarMonitoreo(idUsuario: number) {
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
    this.http.get<any[]>(`/api/Notificaciones/${idUsuario}`)
      .pipe(
        catchError((error) => {
          // Si es 404, devolver array vacío (usuario sin notificaciones)
          if (error.status === 404) {
            return of([]);
          }
          // Otros errores sí los mostramos
          console.error('Error cargando notificaciones:', error);
          return of([]);
        })
      )
      .subscribe({
        next: (notificaciones) => {
          // Mapear respuesta del backend a nuestro modelo
          const mapped = notificaciones.map(n => {
            // Construir información del vehículo si está disponible
            let vehiculoInfo = '';
            if (n.vehiculo) {
              vehiculoInfo = `${n.vehiculo.marca} ${n.vehiculo.modelo}`.trim();
            } else if (n.idSubasta) {
              vehiculoInfo = `Subasta #${n.idSubasta}`;
            }
            
            return {
              id: n.idNotificacion || n.id,
              tipo: this.mapearTipo(n.tipo),
              mensaje: n.mensaje,
              fecha: new Date(n.fechaCreacion || n.fecha),
              leida: n.leida,
              idSubasta: n.idSubasta,
              idVehiculo: n.idVehiculo,
              vehiculoInfo: vehiculoInfo || this.extraerInfoVehiculo(n.mensaje)
            };
          });
          
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
    
    const mapa: any = {
      'PujaGanada': 'puja_ganada',
      'PujaSuperada': 'puja_superada',
      'SubastaFinalizada': 'subasta_finalizada',
      'NuevaSubasta': 'nueva_subasta'
    };
    return mapa[tipo] || tipo.toLowerCase();
  }

  marcarComoLeida(idNotificacion: number) {
    this.http.put(`/api/Notificaciones/${idNotificacion}/leida`, {}).pipe(
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
    return this.http.post('/api/Notificaciones/procesar-finalizadas', {});
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
