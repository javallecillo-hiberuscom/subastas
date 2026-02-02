import { Component, inject, signal, OnInit, ViewChild, ElementRef, NgZone } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { getApiUrl } from '../utils/api-url.helper';

@Component({
  selector: 'app-perfil',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './perfil.component.html',
  styleUrl: './perfil.component.css'
})
export class PerfilComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private router = inject(Router);
  private toast = inject(ToastService);
  private ngZone = inject(NgZone);

  @ViewChild('fotoInput') fotoInput!: ElementRef<HTMLInputElement>;

  currentUser = this.authService.currentUser;
  perfilForm!: FormGroup;
  empresaForm!: FormGroup;
  
  editandoPerfil = signal(false);
  editandoEmpresa = signal(false);
  guardando = signal(false);
  
  mostrarPassword = signal(false);
  empresa = signal<any>(null);
  fotoPreview = signal<string | null>(null);
  archivoFoto: File | null = null;
  isDragging = signal(false);
  console = console; // Para usar console.log en el template

  ngOnInit() {
    const user = this.currentUser();
    if (!user) {
      this.router.navigate(['/login']);
      return;
    }

    // Inicializar formularios
    this.perfilForm = this.fb.group({
      nombre: ['', Validators.required],
      apellidos: [''],
      email: ['', [Validators.required, Validators.email]],
      telefono: [''],
      direccion: [''],
      password: ['']
    });

    this.empresaForm = this.fb.group({
      nombre: ['', Validators.required],
      cif: ['', Validators.required],
      direccion: ['', Validators.required],
      telefono: ['', Validators.required],
      email: ['', [Validators.required, Validators.email]],
      nombreContacto: ['']
    });

    this.perfilForm.disable();
    this.empresaForm.disable();

    // Cargar datos frescos del servidor
    this.cargarPerfil();
  }

  cargarPerfil() {
    const user = this.currentUser();
    if (!user || !user.idUsuario) return;

    this.http.get<any>(`/api/Usuarios/${user.idUsuario}`).subscribe({
      next: (usuarioActualizado) => {
        // Actualizar el usuario en el authService y localStorage
        this.authService.setCurrentUser(usuarioActualizado);
        
        // Actualizar formulario con datos frescos
        this.perfilForm.patchValue({
          nombre: usuarioActualizado.nombre,
          apellidos: usuarioActualizado.apellidos,
          email: usuarioActualizado.email,
          telefono: usuarioActualizado.telefono,
          direccion: usuarioActualizado.direccion
        });

        // Cargar empresa si existe
        if (usuarioActualizado.idEmpresa) {
          this.cargarEmpresa(usuarioActualizado.idEmpresa);
        }
      },
      error: (error) => {
        console.error('Error cargando perfil:', error);
        this.toast.error('Error al cargar el perfil');
        
        // Fallback: usar datos del localStorage
        this.perfilForm.patchValue({
          nombre: user.nombre,
          apellidos: user.apellidos,
          email: user.email,
          telefono: user.telefono,
          direccion: user.direccion
        });
        
        if (user.idEmpresa) {
          this.cargarEmpresa(user.idEmpresa);
        }
      }
    });
  }

  cargarEmpresa(idEmpresa: number) {
    console.log('=== CARGAR EMPRESA ===', idEmpresa);
    this.http.get<any>(getApiUrl(`/api/Empresas/${idEmpresa}`)).subscribe({
      next: (empresa) => {
        console.log('Empresa cargada:', empresa);
        this.empresa.set(empresa);
        this.empresaForm.patchValue({
          nombre: empresa.nombre,
          cif: empresa.cif,
          direccion: empresa.direccion,
          telefono: empresa.telefono,
          email: empresa.email,
          nombreContacto: empresa.nombreContacto
        });
        console.log('Formulario empresa actualizado:', this.empresaForm.getRawValue());
      },
      error: (error) => {
        console.error('Error cargando empresa:', error);
      }
    });
  }

  toggleEditarPerfil() {
    if (this.editandoPerfil()) {
      this.perfilForm.disable();
      this.editandoPerfil.set(false);
      // Restaurar valores originales
      const user = this.currentUser();
      if (user) {
        this.perfilForm.patchValue({
          nombre: user.nombre,
          email: user.email,
          telefono: user.telefono,
          direccion: user.direccion,
          password: ''
        });
      }
    } else {
      this.perfilForm.enable();
      this.perfilForm.get('email')?.disable(); // Email no editable
      this.editandoPerfil.set(true);
    }
  }

  toggleEditarEmpresa() {
    console.log('=== TOGGLE EDITAR EMPRESA ===');
    console.log('Estado actual editando:', this.editandoEmpresa());
    console.log('Valores formulario antes:', this.empresaForm.getRawValue());
    
    if (this.editandoEmpresa()) {
      // Cancelar edición
      const emp = this.empresa();
      console.log('Cancelando edición, restaurando empresa:', emp);
      
      this.empresaForm.disable();
      this.editandoEmpresa.set(false);
      
      // Restaurar valores originales
      if (emp) {
        this.empresaForm.patchValue({
          nombre: emp.nombre,
          cif: emp.cif,
          direccion: emp.direccion,
          telefono: emp.telefono,
          email: emp.email,
          nombreContacto: emp.nombreContacto
        });
        console.log('Valores restaurados:', this.empresaForm.getRawValue());
      }
    } else {
      // Iniciar edición
      console.log('Iniciando edición');
      const valoresActuales = this.empresaForm.getRawValue();
      console.log('Valores actuales antes de enable:', valoresActuales);
      
      this.empresaForm.enable();
      
      // Restaurar valores después de habilitar
      setTimeout(() => {
        this.empresaForm.patchValue(valoresActuales);
        console.log('Valores después de enable y patchValue:', this.empresaForm.getRawValue());
      }, 0);
      
      this.editandoEmpresa.set(true);
    }
  }

  guardarPerfil() {
    console.log('=== GUARDAR PERFIL INICIADO ===');
    console.log('Formulario válido:', this.perfilForm.valid);
    console.log('Formulario valor (value):', this.perfilForm.value);
    console.log('Formulario valor (getRawValue):', this.perfilForm.getRawValue());
    console.log('Formulario status:', this.perfilForm.status);
    console.log('Botón disabled:', this.guardando() || this.perfilForm.invalid);
    
    // Usar getRawValue() para incluir campos disabled
    const formData = this.perfilForm.getRawValue();
    
    // Validar solo los campos editables
    const nombreControl = this.perfilForm.get('nombre');
    if (!nombreControl || nombreControl.invalid) {
      console.log('Nombre inválido');
      this.toast.error('El nombre es requerido');
      return;
    }

    const user = this.currentUser();
    console.log('Usuario actual:', user);
    
    if (!user) {
      console.log('No hay usuario autenticado');
      this.toast.error('No hay usuario autenticado');
      return;
    }

    if (!user.idUsuario) {
      console.log('Usuario sin idUsuario');
      this.toast.error('Error: ID de usuario no encontrado');
      console.error('Usuario sin idUsuario:', user);
      return;
    }

    this.guardando.set(true);
    
    // Construir payload solo con campos primitivos necesarios
    const payload: any = {
      idUsuario: user.idUsuario,
      email: formData.email,
      nombre: formData.nombre,
      apellidos: formData.apellidos || null,
      usuario: formData.email, // Campo requerido por el backend (username)
      rol: user.rol,
      activo: 1 // Byte: 1 = activo, 0 = inactivo
    };

    // Agregar campos opcionales solo si tienen valor
    if (formData.telefono) {
      payload.telefono = formData.telefono;
    }
    
    if (formData.direccion) {
      payload.direccion = formData.direccion;
    }
    
    if (user.idEmpresa) {
      payload.idEmpresa = user.idEmpresa;
    }

    // Si hay password, incluirlo
    if (formData.password) {
      payload.password = formData.password;
    }

    console.log('Guardando perfil con payload:', payload);
    console.log('URL:', `/api/Usuarios/${user.idUsuario}`);

    this.http.put(getApiUrl(`/api/Usuarios/${user.idUsuario}`), payload).subscribe({
      next: (response) => {
        console.log('Respuesta exitosa:', response);
        
        // Recargar datos frescos del servidor
        this.http.get<any>(getApiUrl(`/api/Usuarios/${user.idUsuario}`)).subscribe({
          next: (usuarioActualizado) => {
            this.authService.setCurrentUser(usuarioActualizado);
            
            this.toast.success('Perfil actualizado correctamente');
            this.editandoPerfil.set(false);
            this.perfilForm.disable();
            this.perfilForm.patchValue({password: ''});
            this.guardando.set(false);
          },
          error: () => {
            // Si falla la recarga, actualizar manualmente
            this.authService.setCurrentUser({
              ...user,
              nombre: payload.nombre,
              apellidos: payload.apellidos,
              telefono: payload.telefono,
              direccion: payload.direccion
            });
            
            this.toast.success('Perfil actualizado correctamente');
            this.editandoPerfil.set(false);
            this.perfilForm.disable();
            this.perfilForm.patchValue({password: ''});
            this.guardando.set(false);
          }
        });
      },
      error: (error) => {
        console.error('Error completo:', error);
        console.error('Error status:', error.status);
        console.error('Error message:', error.message);
        console.error('Error body:', error.error);
        console.error('Error validación errors:', error.error?.errors);
        
        // Mostrar errores específicos de validación
        let mensajeError = 'Error al actualizar el perfil';
        if (error.error?.errors) {
          const errores = Object.keys(error.error.errors).map(key => 
            `${key}: ${error.error.errors[key].join(', ')}`
          ).join('; ');
          mensajeError = `Errores de validación: ${errores}`;
          console.error('Errores formateados:', errores);
        }
        
        this.toast.error(mensajeError, 5000);
        this.guardando.set(false);
      }
    });
  }

  guardarEmpresa() {
    console.log('=== GUARDAR EMPRESA INICIADO ===');
    
    const formValue = this.empresaForm.getRawValue();
    console.log('Valores del formulario:', formValue);
    console.log('Formulario válido:', this.empresaForm.valid);
    console.log('Formulario estado:', this.empresaForm.status);
    
    // Validar campos requeridos
    if (!formValue.nombre?.trim() || !formValue.cif?.trim() || 
        !formValue.direccion?.trim() || !formValue.telefono?.trim() || 
        !formValue.email?.trim()) {
      console.log('Validación fallida - campos vacíos');
      this.toast.error('Por favor completa todos los campos requeridos');
      return;
    }

    const user = this.currentUser();
    console.log('Usuario actual:', user);
    
    if (!user) {
      console.log('No hay usuario autenticado');
      this.toast.error('No hay usuario autenticado');
      return;
    }

    this.guardando.set(true);
    console.log('Guardando empresa...');

    const payload = formValue;
    console.log('Payload:', payload);

    if (user.idEmpresa) {
      console.log('Actualizando empresa existente:', user.idEmpresa);
      // Actualizar empresa existente
      this.http.put(getApiUrl(`/api/Empresas/${user.idEmpresa}`), {...payload, idEmpresa: user.idEmpresa}).subscribe({
        next: () => {
          console.log('Empresa actualizada correctamente');
          this.toast.success('Empresa actualizada correctamente');
          this.editandoEmpresa.set(false);
          this.empresaForm.disable();
          this.guardando.set(false);
          this.cargarEmpresa(user.idEmpresa!);
        },
        error: (error) => {
          console.error('Error actualizando empresa:', error);
          this.toast.error('Error al actualizar la empresa');
          this.guardando.set(false);
        }
      });
    } else {
      // Crear nueva empresa
      this.http.post(getApiUrl('/api/Empresas'), payload).subscribe({
        next: (response: any) => {
          // Actualizar usuario con idEmpresa
          this.http.put(getApiUrl(`/api/Usuarios/${user.idUsuario}`), {...user, idEmpresa: response.idEmpresa}).subscribe({
            next: () => {
              this.authService.setCurrentUser({...user, idEmpresa: response.idEmpresa});
              this.toast.success('Empresa creada correctamente');
              this.editandoEmpresa.set(false);
              this.empresaForm.disable();
              this.guardando.set(false);
              this.cargarEmpresa(response.idEmpresa);
            },
            error: (error) => {
              console.error('Error vinculando empresa:', error);
              this.guardando.set(false);
            }
          });
        },
        error: (error) => {
          console.error('Error creando empresa:', error);
          this.toast.error('Error al crear la empresa');
          this.guardando.set(false);
        }
      });
    }
  }

  togglePassword() {
    this.mostrarPassword.update(v => !v);
  }

  abrirSelectorFoto() {
    console.log('=== abrirSelectorFoto EJECUTADO ===');
    console.log('fotoInput:', this.fotoInput);
    console.log('fotoInput.nativeElement:', this.fotoInput?.nativeElement);
    
    if (this.fotoInput?.nativeElement) {
      console.log('Intentando hacer click en input...');
      const inputElement = this.fotoInput.nativeElement;
      console.log('Input element:', inputElement);
      console.log('Input type:', inputElement.type);
      console.log('Input display:', window.getComputedStyle(inputElement).display);
      
      // Intento 1: Click directo
      inputElement.click();
      console.log('Click ejecutado');
      
      // Intento 2: Si no funciona, crear y disparar evento click manualmente
      setTimeout(() => {
        const event = new MouseEvent('click', {
          view: window,
          bubbles: true,
          cancelable: true
        });
        inputElement.dispatchEvent(event);
        console.log('Evento click disparado manualmente');
      }, 100);
    } else {
      console.error('fotoInput no está disponible');
    }
  }

  // SOLUCIÓN: Ejecutar click del input FUERA de Zone.js para evitar interferencias
  clickInputFoto() {
    // Ejecutar fuera de Angular Zone para evitar que Zone.js interfiera con el user gesture
    this.ngZone.runOutsideAngular(() => {
      if (this.fotoInput?.nativeElement) {
        this.fotoInput.nativeElement.click();
      }
    });
  }

  // Drag & Drop handlers
  onDragOver(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(true);
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);
  }

  async onDrop(event: DragEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.isDragging.set(false);

    const files = event.dataTransfer?.files;
    if (files && files.length > 0) {
      const file = files[0];
      await this.procesarArchivo(file);
    }
  }

  async procesarArchivo(file: File) {
    // Validar tipo de archivo
    if (!file.type.startsWith('image/')) {
      this.toast.error('Por favor selecciona una imagen válida');
      return;
    }

    try {
      // Redimensionar y comprimir imagen
      const imagenComprimida = await this.comprimirImagen(file);
      this.archivoFoto = imagenComprimida;

      // Crear preview
      const reader = new FileReader();
      reader.onload = (e) => {
        this.fotoPreview.set(e.target?.result as string);
      };
      reader.readAsDataURL(imagenComprimida);
    } catch (error) {
      console.error('Error procesando imagen:', error);
      this.toast.error('Error al procesar la imagen');
    }
  }

  async onFotoSeleccionada(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      await this.procesarArchivo(input.files[0]);
    }
  }

  eliminarFoto() {
    this.archivoFoto = null;
    this.fotoPreview.set(null);
    
    const user = this.currentUser();
    if (!user || !user.idUsuario) {
      this.toast.error('No hay usuario autenticado');
      return;
    }

    this.guardando.set(true);

    this.http.delete(getApiUrl(`/api/Usuarios/${user.idUsuario}/foto-perfil`)).subscribe({
      next: () => {
        // Recargar usuario desde servidor
        this.http.get<any>(getApiUrl(`/api/Usuarios/${user.idUsuario}`)).subscribe({
          next: (usuarioActualizado) => {
            this.authService.setCurrentUser(usuarioActualizado);
            this.toast.success('Foto eliminada correctamente');
            this.guardando.set(false);
          },
          error: () => {
            // Fallback: actualizar manualmente
            if (user) {
              this.authService.setCurrentUser({
                ...user,
                fotoPerfilBase64: undefined
              });
            }
            this.toast.success('Foto eliminada correctamente');
            this.guardando.set(false);
          }
        });
      },
      error: (error: any) => {
        console.error('Error eliminando foto:', error);
        this.toast.error('Error al eliminar la foto');
        this.guardando.set(false);
      }
    });
  }

  async guardarFoto() {
    if (!this.archivoFoto) return;

    const user = this.currentUser();
    if (!user || !user.idUsuario) {
      this.toast.error('No hay usuario autenticado');
      return;
    }

    this.guardando.set(true);

    try {
      const base64 = await this.convertirABase64(this.archivoFoto);

      this.http.put(getApiUrl(`/api/Usuarios/${user.idUsuario}/foto-perfil`), { 
        fotoBase64: base64 
      }).subscribe({
        next: () => {
          // Recargar el usuario completo desde el servidor para obtener la foto actualizada
          this.http.get<any>(getApiUrl(`/api/Usuarios/${user.idUsuario}`)).subscribe({
            next: (usuarioActualizado) => {
              // Actualizar en authService y localStorage
              this.authService.setCurrentUser(usuarioActualizado);
              
              this.toast.success('Foto actualizada correctamente');
              this.archivoFoto = null;
              this.fotoPreview.set(null);
              this.guardando.set(false);
            },
            error: (errorGet) => {
              console.error('Error recargando usuario:', errorGet);
              // Aunque falle la recarga, la foto se guardó
              this.toast.success('Foto actualizada. Recarga la página para verla.');
              this.archivoFoto = null;
              this.fotoPreview.set(null);
              this.guardando.set(false);
            }
          });
        },
        error: (error) => {
          console.error('Error actualizando foto:', error);
          this.toast.error('Error al actualizar la foto');
          this.guardando.set(false);
        }
      });
    } catch (error) {
      console.error('Error convirtiendo imagen:', error);
      this.toast.error('Error al procesar la imagen');
      this.guardando.set(false);
    }
  }

  private convertirABase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.readAsDataURL(file);
      reader.onload = () => {
        const result = reader.result as string;
        // Extraer solo el base64 sin el prefijo data:image/...;base64,
        const base64 = result.split(',')[1];
        resolve(base64);
      };
      reader.onerror = error => reject(error);
    });
  }

  private comprimirImagen(file: File): Promise<File> {
    return new Promise((resolve, reject) => {
      const img = new Image();
      const canvas = document.createElement('canvas');
      const ctx = canvas.getContext('2d');

      img.onload = () => {
        // Dimensiones máximas para la imagen
        const maxWidth = 400;
        const maxHeight = 400;
        
        let width = img.width;
        let height = img.height;

        // Calcular nuevas dimensiones manteniendo proporción
        if (width > height) {
          if (width > maxWidth) {
            height = Math.round((height * maxWidth) / width);
            width = maxWidth;
          }
        } else {
          if (height > maxHeight) {
            width = Math.round((width * maxHeight) / height);
            height = maxHeight;
          }
        }

        canvas.width = width;
        canvas.height = height;

        // Dibujar imagen redimensionada
        ctx?.drawImage(img, 0, 0, width, height);

        // Convertir a Blob con compresión JPEG (calidad 0.7)
        canvas.toBlob(
          (blob) => {
            if (blob) {
              // Convertir Blob a File
              const compressedFile = new File([blob], file.name, {
                type: 'image/jpeg',
                lastModified: Date.now()
              });

              // Verificar que el tamaño final en base64 no exceda ~60KB
              // (aprox 80KB en base64 = ~60KB en varchar)
              if (blob.size > 60 * 1024) {
                // Si aún es muy grande, comprimir más
                canvas.toBlob(
                  (blob2) => {
                    if (blob2) {
                      const finalFile = new File([blob2], file.name, {
                        type: 'image/jpeg',
                        lastModified: Date.now()
                      });
                      resolve(finalFile);
                    } else {
                      reject(new Error('Error al comprimir imagen'));
                    }
                  },
                  'image/jpeg',
                  0.5 // Compresión mayor
                );
              } else {
                resolve(compressedFile);
              }
            } else {
              reject(new Error('Error al convertir imagen'));
            }
          },
          'image/jpeg',
          0.7 // Calidad 70%
        );
      };

      img.onerror = () => reject(new Error('Error al cargar imagen'));
      img.src = URL.createObjectURL(file);
    });
  }
}
