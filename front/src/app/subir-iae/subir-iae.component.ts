import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Router, RouterModule } from '@angular/router';
import { ToastService } from '../services/toast.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-subir-iae',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './subir-iae.component.html',
  styleUrl: './subir-iae.component.css'
})
export class SubirIaeComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private authService = inject(AuthService);
  private router = inject(Router);

  archivoSeleccionado = signal<File | null>(null);
  cargando = signal(false);
  estadoValidacion = signal<{tieneDocumento: boolean, validado: boolean} | null>(null);
  dragOver = signal(false);

  currentUser = this.authService.currentUser;

  ngOnInit(): void {
    this.verificarEstado();
  }

  verificarEstado(): void {
    const user = this.currentUser();
    if (!user?.idUsuario) return;

    this.http.get<any>(`/api/Documentos/verificar-iae/${user.idUsuario}`)
      .subscribe({
        next: (estado) => {
          this.estadoValidacion.set(estado);
        },
        error: (error) => {
          console.error('Error verificando estado:', error);
        }
      });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      
      console.log('Archivo seleccionado:', file.name, file.type, file.size);
      
      // Validar tipo de archivo
      const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png', 'image/jpg'];
      if (!allowedTypes.includes(file.type)) {
        this.toast.error('Solo se permiten archivos PDF o imágenes (JPG, PNG)');
        input.value = '';
        return;
      }

      // Validar tamaño (10MB máximo)
      if (file.size > 10 * 1024 * 1024) {
        this.toast.error('El archivo no puede superar los 10MB');
        input.value = '';
        return;
      }

      this.archivoSeleccionado.set(file);
      console.log('Archivo aceptado, signal actualizado');
    }
  }

  subirDocumento(): void {
    const archivo = this.archivoSeleccionado();
    const user = this.currentUser();

    if (!archivo || !user?.idUsuario) {
      this.toast.error('Debes seleccionar un archivo');
      return;
    }

    this.cargando.set(true);

    // Convertir archivo a Base64
    const reader = new FileReader();
    reader.onload = () => {
      const base64String = (reader.result as string).split(',')[1];

      const request = {
        documentoBase64: base64String,
        nombreArchivo: archivo.name
      };

      this.http.post(`/api/Documentos/subir-iae/${user.idUsuario}`, request)
        .subscribe({
          next: (response: any) => {
            this.toast.success('Documento subido correctamente. Pendiente de validación por el administrador.');
            this.archivoSeleccionado.set(null);
            this.cargando.set(false);
            this.verificarEstado();
          },
          error: (error) => {
            console.error('Error subiendo documento:', error);
            this.toast.error(error.error?.mensaje || 'Error al subir el documento');
            this.cargando.set(false);
          }
        });
    };

    reader.readAsDataURL(archivo);
  }

  limpiarArchivo(): void {
    this.archivoSeleccionado.set(null);
    // Resetear el input file
    const fileInput = document.getElementById('fileInput') as HTMLInputElement;
    if (fileInput) {
      fileInput.value = '';
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(true);
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    event.stopPropagation();
    this.dragOver.set(false);

    if (this.cargando()) return;

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      const file = files[0];
      
      console.log('Archivo arrastrado:', file.name, file.type, file.size);
      
      // Validar tipo de archivo
      const allowedTypes = ['application/pdf', 'image/jpeg', 'image/png', 'image/jpg'];
      if (!allowedTypes.includes(file.type)) {
        this.toast.error('Solo se permiten archivos PDF o imágenes (JPG, PNG)');
        return;
      }

      // Validar tamaño (10MB máximo)
      if (file.size > 10 * 1024 * 1024) {
        this.toast.error('El archivo no puede superar los 10MB');
        return;
      }

      this.archivoSeleccionado.set(file);
      console.log('Archivo aceptado, signal actualizado');
    }
  }

  getEstadoMensaje(): string {
    const estado = this.estadoValidacion();
    if (!estado) return '';
    
    if (estado.validado) return '✅ Tu cuenta está validada. Puedes realizar pujas.';
    if (estado.tieneDocumento) return '⏳ Tu documento está pendiente de validación por un administrador.';
    return '📄 Debes subir tu documento IAE para poder pujar.';
  }

  getEstadoClass(): string {
    const estado = this.estadoValidacion();
    if (!estado) return '';
    
    if (estado.validado) return 'estado-validado';
    if (estado.tieneDocumento) return 'estado-pendiente';
    return 'estado-sin-documento';
  }
}
