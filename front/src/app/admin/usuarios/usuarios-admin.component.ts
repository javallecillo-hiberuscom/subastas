import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { getApiUrl } from '../../utils/api-url.helper';

interface UsuarioAdmin {
  idUsuario: number;
  email: string;
  nombre: string;
  dni?: string;
  rol: string;
  cif?: string;
  direccionEmpresa?: string;
  telefonoContacto?: string;
  documentoIAE?: string;
  fechaRegistro: Date;
  activo: boolean;
  idEmpresa?: number;
  nombreEmpresa?: string;
}

@Component({
  selector: 'app-usuarios-admin',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './usuarios-admin.component.html',
  styleUrl: './usuarios-admin.component.css'
})
export class UsuariosAdminComponent implements OnInit {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  
  usuarios = signal<UsuarioAdmin[]>([]);
  loading = signal(true);
  filtro = signal<'todos' | 'registrado' | 'validado' | 'administrador'>('todos');
  busqueda = signal('');
  
  mostrarFormulario = signal(false);
  usuarioEditando = signal<UsuarioAdmin | null>(null);
  usuarioForm!: FormGroup;
  guardando = signal(false);
  mensaje = signal('');
  tipoMensaje = signal<'success' | 'error'>('success');

  ngOnInit() {
    this.inicializarFormulario();
    this.cargarUsuarios();
  }
  
  inicializarFormulario() {
    this.usuarioForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      nombre: ['', Validators.required],
      dni: ['', [Validators.required, Validators.pattern(/^[0-9]{8}[A-Z]$/)]],
      rol: ['registrado', Validators.required],
      cif: ['', Validators.pattern(/^[A-Z][0-9]{8}$/)],
      direccionEmpresa: [''],
      telefonoContacto: ['', Validators.pattern(/^[0-9]{9}$/)]
    });
  }

  cargarUsuarios() {
    this.loading.set(true);
    this.http.get<UsuarioAdmin[]>(getApiUrl('/api/Usuarios')).subscribe({
      next: (usuarios) => {
        this.usuarios.set(usuarios);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error cargando usuarios:', error);
        this.mensaje.set('Error al cargar usuarios');
        this.tipoMensaje.set('error');
        this.loading.set(false);
      }
    });
  }

  usuariosFiltrados() {
    let usuarios = this.usuarios();
    
    // Filtrar por rol
    if (this.filtro() !== 'todos') {
      usuarios = usuarios.filter(u => u.rol === this.filtro());
    }
    
    // Filtrar por búsqueda
    const busquedaLower = this.busqueda().toLowerCase();
    if (busquedaLower) {
      usuarios = usuarios.filter(u => 
        u.nombre?.toLowerCase().includes(busquedaLower) ||
        u.email?.toLowerCase().includes(busquedaLower) ||
        u.dni?.toLowerCase().includes(busquedaLower) ||
        u.cif?.toLowerCase().includes(busquedaLower)
      );
    }
    
    return usuarios;
  }

  cambiarFiltro(filtro: 'todos' | 'registrado' | 'validado' | 'administrador') {
    this.filtro.set(filtro);
  }

  nuevoUsuario() {
    this.usuarioEditando.set(null);
    this.usuarioForm.reset({ rol: 'registrado' });
    this.mostrarFormulario.set(true);
    this.mensaje.set('');
  }

  editarUsuario(usuario: UsuarioAdmin) {
    this.usuarioEditando.set(usuario);
    this.usuarioForm.patchValue(usuario);
    this.mostrarFormulario.set(true);
    this.mensaje.set('');
  }

  cerrarFormulario() {
    this.mostrarFormulario.set(false);
    this.usuarioEditando.set(null);
    this.usuarioForm.reset({ rol: 'registrado' });
    this.mensaje.set('');
  }

  guardarUsuario() {
    if (this.usuarioForm.invalid) {
      Object.keys(this.usuarioForm.controls).forEach(key => {
        this.usuarioForm.get(key)?.markAsTouched();
      });
      return;
    }

    this.guardando.set(true);
    const datosUsuario = this.usuarioForm.value;

    if (this.usuarioEditando()) {
      // Actualizar usuario existente
      const id = this.usuarioEditando()!.idUsuario;
      this.http.put(getApiUrl(`/api/Usuarios/${id}`), datosUsuario).subscribe({
        next: () => {
          this.mensaje.set('Usuario actualizado correctamente');
          this.tipoMensaje.set('success');
          this.guardando.set(false);
          this.cargarUsuarios();
          setTimeout(() => this.cerrarFormulario(), 1500);
        },
        error: (error) => {
          console.error('Error actualizando usuario:', error);
          this.mensaje.set('Error al actualizar usuario');
          this.tipoMensaje.set('error');
          this.guardando.set(false);
        }
      });
    } else {
      // Crear nuevo usuario
      this.http.post(getApiUrl('/api/Usuarios'), datosUsuario).subscribe({
        next: () => {
          this.mensaje.set('Usuario creado correctamente');
          this.tipoMensaje.set('success');
          this.guardando.set(false);
          this.cargarUsuarios();
          setTimeout(() => this.cerrarFormulario(), 1500);
        },
        error: (error) => {
          console.error('Error creando usuario:', error);
          this.mensaje.set('Error al crear usuario');
          this.tipoMensaje.set('error');
          this.guardando.set(false);
        }
      });
    }
  }

  validarUsuario(usuario: UsuarioAdmin) {
    if (!confirm(`¿Validar al usuario ${usuario.nombre}?`)) return;

    this.http.put(getApiUrl(`/api/Usuarios/${usuario.idUsuario}/validar`), {}).subscribe({
      next: () => {
        this.mensaje.set('Usuario validado correctamente');
        this.tipoMensaje.set('success');
        this.cargarUsuarios();
        setTimeout(() => this.mensaje.set(''), 3000);
      },
      error: (error) => {
        console.error('Error validando usuario:', error);
        this.mensaje.set('Error al validar usuario');
        this.tipoMensaje.set('error');
        setTimeout(() => this.mensaje.set(''), 3000);
      }
    });
  }

  toggleActivo(usuario: UsuarioAdmin) {
    const accion = usuario.activo ? 'desactivar' : 'activar';
    if (!confirm(`¿${accion.charAt(0).toUpperCase() + accion.slice(1)} al usuario ${usuario.nombre}?`)) return;

    this.http.put(getApiUrl(`/api/Usuarios/${usuario.idUsuario}`), { ...usuario, activo: !usuario.activo }).subscribe({
      next: () => {
        this.mensaje.set(`Usuario ${accion}do correctamente`);
        this.tipoMensaje.set('success');
        this.cargarUsuarios();
        setTimeout(() => this.mensaje.set(''), 3000);
      },
      error: (error) => {
        console.error(`Error al ${accion} usuario:`, error);
        this.mensaje.set(`Error al ${accion} usuario`);
        this.tipoMensaje.set('error');
        setTimeout(() => this.mensaje.set(''), 3000);
      }
    });
  }

  eliminarUsuario(usuario: UsuarioAdmin) {
    if (!confirm(`¿Eliminar definitivamente al usuario ${usuario.nombre}? Esta acción no se puede deshacer.`)) return;

    this.http.delete(getApiUrl(`/api/Usuarios/${usuario.idUsuario}`)).subscribe({
      next: () => {
        this.mensaje.set('Usuario eliminado correctamente');
        this.tipoMensaje.set('success');
        this.cargarUsuarios();
        setTimeout(() => this.mensaje.set(''), 3000);
      },
      error: (error) => {
        console.error('Error eliminando usuario:', error);
        this.mensaje.set('Error al eliminar usuario');
        this.tipoMensaje.set('error');
        setTimeout(() => this.mensaje.set(''), 3000);
      }
    });
  }

  descargarDocumento(url: string) {
    window.open(url, '_blank');
  }

  obtenerChipEstado(rol: string): string {
    switch(rol) {
      case 'registrado': return 'chip-registrado';
      case 'validado': return 'chip-validado';
      case 'administrador': return 'chip-admin';
      default: return '';
    }
  }

  obtenerTextoEstado(rol: string): string {
    switch(rol) {
      case 'registrado': return 'Pendiente';
      case 'validado': return 'Validado';
      case 'administrador': return 'Admin';
      default: return rol;
    }
  }
}
