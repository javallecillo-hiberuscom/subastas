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
    console.log('NotificationService: Iniciando monitoreo para usuario:', idUsuario, 'esAdmin:', esAdmin);
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
    
    console.log('NotificationService: Cargando notificaciones desde:', url, 'esAdmin:', this.esAdmin);
    
    this.http.get<any[]>(url)
      .pipe(
        catchError((error) => {
          // Si es 404, devolver array vacío (usuario sin notificaciones)
          if (error.status === 404) {
            console.log('NotificationService: Usuario sin notificaciones (404)');
            return of([]);
          }
          // Otros errores sí los mostramos
          console.error('NotificationService: Error cargando notificaciones:', error);
          console.error('NotificationService: Status:', error.status, 'URL:', url);
          return of([]);
        })
      )
      .subscribe({
        next: (notificaciones) => {
          console.log('NotificationService: Notificaciones recibidas RAW:', notificaciones);
          console.log('NotificationService: Tipo de respuesta:', typeof notificaciones, 'Es array:', Array.isArray(notificaciones));
          
          // Asegurarse de que sea un array
          const notifArray = Array.isArray(notificaciones) ? notificaciones : [];
          
          // Mapear respuesta del backend a nuestro modelo
          const mapped = notifArray.map(n => {
            // Construir información del vehículo si está disponible
            let vehiculoInfo = '';
            if (n.vehiculo) {
              vehiculoInfo = `${n.vehiculo.marca} ${n.vehiculo.modelo}`.trim();
            } else if (n.idSubasta) {
              vehiculoInfo = `Subasta #${n.idSubasta}`;
            }
            
            // Obtener mensaje correcto según estructura (admin usa Mensaje, usuario usa mensaje)
            let mensajeBase = '';
            if (this.esAdmin) {
              // NotificacionAdmin tiene Titulo y Mensaje con mayúscula
              mensajeBase = n.Mensaje || n.Titulo || n.mensaje || n.titulo || '';
            } else {
              // Notificacion de usuario tiene mensaje con minúscula
              mensajeBase = n.mensaje || n.Mensaje || n.titulo || n.Titulo || '';
            }
            
            // Para notificaciones admin, agregar info del usuario si existe
            if (this.esAdmin && n.Usuario) {
              const nombreUsuario = `${n.Usuario.nombre || n.Usuario.Nombre || ''} ${n.Usuario.apellidos || n.Usuario.Apellidos || ''}`.trim() || n.Usuario.email || n.Usuario.Email;
              if (nombreUsuario && !mensajeBase.includes(nombreUsuario)) {
                mensajeBase = `${nombreUsuario}: ${mensajeBase}`;
              }
            }
            
            // Determinar si está leída (NotificacionAdmin usa Leida como byte 0/1)
            let estaLeida = false;
            const leidaValue = n.Leida ?? n.leida;
            
            if (typeof leidaValue === 'boolean') {
              estaLeida = leidaValue;
            } else if (typeof leidaValue === 'number') {
              estaLeida = leidaValue === 1;
            }
            
            console.log('NotificationService: Mapeo notif ID:', n.IdNotificacion || n.idNotificacion, 'Tipo:', n.Tipo || n.tipo, 'Leida raw:', leidaValue, 'Leida calculada:', estaLeida);
            
            return {
              id: n.IdNotificacion || n.idNotificacion || n.id,
              tipo: this.mapearTipo(n.Tipo || n.tipo || ''),
              mensaje: mensajeBase,
              fecha: new Date(n.FechaCreacion || n.fechaCreacion || n.fecha || n.FechaEnvio),
              leida: estaLeida,
              idSubasta: n.idSubasta || n.IdSubasta,
              idVehiculo: n.idVehiculo || n.IdVehiculo,
              vehiculoInfo: vehiculoInfo || this.extraerInfoVehiculo(mensajeBase),
              idUsuario: n.IdUsuario || n.idUsuario // Para poder navegar en admin
            };
          });
          
          console.log('NotificationService: Notificaciones mapeadas:', mapped);
          console.log('NotificationService: Total mapeadas:', mapped.length);
          console.log('NotificationService: No leídas:', mapped.filter(n => !n.leida).length);
          
          this.notificaciones.set(mapped);
          this.noLeidas.set(mapped.filter(n => !n.leida).length);
          
          console.log('NotificationService: Signal notificaciones actualizada, total:', this.notificaciones().length);
          console.log('NotificationService: Signal noLeidas actualizada:', this.noLeidas());
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
