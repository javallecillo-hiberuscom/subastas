import { Injectable, signal } from '@angular/core';

export interface Toast {
  id: number;
  tipo: 'success' | 'error' | 'info' | 'warning';
  mensaje: string;
  duracion?: number;
}

@Injectable({
  providedIn: 'root'
})
export class ToastService {
  private toasts = signal<Toast[]>([]);
  private nextId = 1;

  getToasts = this.toasts.asReadonly();

  mostrar(tipo: Toast['tipo'], mensaje: string, duracion: number = 3000) {
    const toast: Toast = {
      id: this.nextId++,
      tipo,
      mensaje,
      duracion
    };

    this.toasts.update(toasts => [...toasts, toast]);

    if (duracion > 0) {
      setTimeout(() => this.cerrar(toast.id), duracion);
    }
  }

  success(mensaje: string, duracion?: number) {
    this.mostrar('success', mensaje, duracion);
  }

  error(mensaje: string, duracion?: number) {
    this.mostrar('error', mensaje, duracion);
  }

  info(mensaje: string, duracion?: number) {
    this.mostrar('info', mensaje, duracion);
  }

  warning(mensaje: string, duracion?: number) {
    this.mostrar('warning', mensaje, duracion);
  }

  cerrar(id: number) {
    this.toasts.update(toasts => toasts.filter(t => t.id !== id));
  }
}
