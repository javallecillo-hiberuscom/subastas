import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ToastService } from '../../services/toast.service';
import { getApiUrl } from '../../utils/api-url.helper';

interface Usuario {
  idUsuario: number;
  nombre: string;
  apellidos: string;
  email: string;
  rol: string;
  activo: number;
  validado: boolean;
  telefono?: string;
  direccion?: string;
  documentoIAE?: string;
  idEmpresa: number;
}

@Component({
  selector: 'app-usuarios',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './usuarios.component.html',
  styleUrl: './usuarios.component.css'
})
export class UsuariosComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);

  usuarios = signal<Usuario[]>([]);
  loading = signal(true);
  usuarioSeleccionado = signal<Usuario | null>(null);

  ngOnInit(): void {
    this.cargarUsuarios();
  }

  cargarUsuarios(): void {
    this.loading.set(true);
    this.http.get<Usuario[]>(getApiUrl('/api/Usuarios')).subscribe({
      next: (usuarios) => {
        this.usuarios.set(usuarios);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error cargando usuarios:', error);
        this.toast.error('Error al cargar usuarios');
        this.loading.set(false);
      }
    });
  }

  validarUsuario(usuario: Usuario): void {
    const nuevoEstado = !usuario.validado;
    
    this.http.put(getApiUrl(`/api/Usuarios/${usuario.idUsuario}/validar`), { Validado: nuevoEstado })
      .subscribe({
        next: () => {
          usuario.validado = nuevoEstado;
          this.toast.success(nuevoEstado ? 'Usuario validado correctamente' : 'Validación revocada');
        },
        error: (error) => {
          console.error('Error validando usuario:', error);
          this.toast.error('Error al validar usuario');
        }
      });
  }

  descargarDocumento(usuario: Usuario): void {
    if (!usuario.documentoIAE) {
      this.toast.warning('Este usuario no ha subido ningún documento');
      return;
    }

    this.http.get(getApiUrl(`/api/Documentos/descargar-iae/${usuario.idUsuario}`), {
      responseType: 'blob'
    }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const link = document.createElement('a');
        link.href = url;
        link.download = `IAE_${usuario.nombre}_${usuario.apellidos}.pdf`;
        link.click();
        window.URL.revokeObjectURL(url);
        this.toast.success('Documento descargado');
      },
      error: (error) => {
        console.error('Error descargando documento:', error);
        this.toast.error('Error al descargar el documento');
      }
    });
  }

  cambiarEstadoActivo(usuario: Usuario): void {
    const nuevoEstado = usuario.activo === 1 ? 0 : 1;
    const request = {
      idUsuario: usuario.idUsuario,
      nombre: usuario.nombre,
      apellidos: usuario.apellidos,
      email: usuario.email,
      rol: usuario.rol,
      activo: nuevoEstado,
      telefono: usuario.telefono,
      direccion: usuario.direccion,
      idEmpresa: usuario.idEmpresa
    };

    this.http.put(getApiUrl(`/api/Usuarios/${usuario.idUsuario}`), request)
      .subscribe({
        next: () => {
          usuario.activo = nuevoEstado;
          this.toast.success(nuevoEstado === 1 ? 'Usuario activado' : 'Usuario desactivado');
        },
        error: (error) => {
          console.error('Error cambiando estado:', error);
          this.toast.error('Error al cambiar el estado del usuario');
        }
      });
  }

  eliminarUsuario(usuario: Usuario): void {
    if (!confirm(`¿Estás seguro de eliminar al usuario ${usuario.nombre} ${usuario.apellidos}?`)) {
      return;
    }

    this.http.delete(getApiUrl(`/api/Usuarios/${usuario.idUsuario}`))
      .subscribe({
        next: () => {
          this.usuarios.update(users => users.filter(u => u.idUsuario !== usuario.idUsuario));
          this.toast.success('Usuario eliminado correctamente');
        },
        error: (error) => {
          console.error('Error eliminando usuario:', error);
          this.toast.error('Error al eliminar el usuario');
        }
      });
  }

  getEstadoBadgeClass(usuario: Usuario): string {
    if (usuario.validado) return 'badge-validado';
    if (usuario.documentoIAE) return 'badge-pendiente';
    return 'badge-sin-documento';
  }

  getEstadoTexto(usuario: Usuario): string {
    if (usuario.validado) return 'Validado';
    if (usuario.documentoIAE) return 'Pendiente';
    return 'Sin Documento';
  }
}
