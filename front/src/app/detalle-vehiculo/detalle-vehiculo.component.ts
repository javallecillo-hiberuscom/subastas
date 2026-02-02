import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { ToastService } from '../services/toast.service';
import { getApiUrl } from '../utils/api-url.helper';

interface Subasta {
  idSubasta: number;
  idVehiculo: number;
  fechaInicio: string;
  fechaFin: string;
  precioInicial: number;
  precioActual: number;
  incrementoMinimo: number;
  estado: string;
  vehiculo: any;
}

@Component({
  selector: 'app-detalle-vehiculo',
  imports: [CommonModule, FormsModule],
  templateUrl: './detalle-vehiculo.component.html',
  styleUrl: './detalle-vehiculo.component.css'
})
export class DetalleVehiculoComponent implements OnInit, OnDestroy {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private http = inject(HttpClient);
  private authService = inject(AuthService);
  private toast = inject(ToastService);

  subasta = signal<Subasta | null>(null);
  imagenSeleccionada = signal(0);
  cantidadPuja = signal<number>(0);
  isLoading = signal(true);
  isPujando = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  currentUser = this.authService.currentUser;
  private refreshInterval: any;
  private carouselInterval: any;
  autoplayEnabled = signal(true);

  // Computed: número total de imágenes
  totalImagenes = computed(() => {
    return this.subasta()?.vehiculo?.imagenes?.length || 0;
  });

  // Computed: subasta activa (dentro del rango de fechas)
  subastaActiva = computed(() => {
    const sub = this.subasta();
    if (!sub) return false;
    
    const ahora = new Date();
    const inicio = new Date(sub.fechaInicio);
    const fin = new Date(sub.fechaFin);
    
    return ahora >= inicio && ahora <= fin && sub.estado === 'activa';
  });

  // Computed: tiempo restante
  tiempoRestante = computed(() => {
    const sub = this.subasta();
    if (!sub) return '';
    
    const ahora = new Date();
    const fin = new Date(sub.fechaFin);
    const diff = fin.getTime() - ahora.getTime();
    
    if (diff <= 0) {
      // Subasta finalizada - procesar notificaciones si aún está activa
      if (sub.estado === 'activa') {
        this.procesarSubastaFinalizada();
      }
      return 'Finalizada';
    }
    
    const dias = Math.floor(diff / (1000 * 60 * 60 * 24));
    const horas = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutos = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    
    if (dias > 0) return `${dias}d ${horas}h ${minutos}m`;
    if (horas > 0) return `${horas}h ${minutos}m`;
    return `${minutos}m`;
  });

  // Computed: puede pujar
  puedePujar = computed(() => {
    const user = this.currentUser();
    return this.subastaActiva() && (user?.rol === 'validado' || user?.rol === 'administrador');
  });

  // Computed: es admin
  esAdmin = computed(() => {
    const user = this.currentUser();
    return user?.rol?.toLowerCase() === 'administrador';
  });

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.cargarSubasta(id);
      // Refrescar precio cada 5 segundos si la subasta está activa
      this.refreshInterval = setInterval(() => {
        if (this.subastaActiva()) {
          this.cargarSubasta(id, true);
        } else {
          clearInterval(this.refreshInterval);
        }
      }, 5000);
    }
  }

  ngOnDestroy(): void {
    if (this.refreshInterval) {
      clearInterval(this.refreshInterval);
    }
    this.detenerAutoplay();
  }

  cargarSubasta(idSubasta: number, silent: boolean = false): void {
    if (!silent) {
      this.isLoading.set(true);
    }
    // Buscar la subasta activa
    this.http.get<Subasta[]>(getApiUrl('/api/Subastas?activas=true')).subscribe({
      next: (subastas) => {
        const precioAnterior = this.subasta()?.precioActual;
        const subasta = subastas.find(s => s.idSubasta === idSubasta);
        
        if (subasta) {
          this.subasta.set(subasta);
                    // Iniciar autoplay si hay m\u00faltiples im\u00e1genes
          if (!silent && subasta.vehiculo?.imagenes?.length > 1) {
            setTimeout(() => this.iniciarAutoplay(), 1000);
          }
                    // Mostrar notificación si el precio cambió (solo en refrescos automáticos)
          // Y solo si NO estamos en proceso de pujar (evitar duplicar notificación)
          if (silent && precioAnterior && subasta.precioActual > precioAnterior && !this.isPujando()) {
            this.toast.info(`El precio ha subido a ${subasta.precioActual.toLocaleString()}€`);
          }
          
          // Actualizar cantidad sugerida al nuevo precio actual + 1
          this.cantidadPuja.set(subasta.precioActual + 1);
        } else {
          if (!silent) {
            this.errorMessage.set('No se encontró una subasta activa');
          }
        }
        
        if (!silent) {
          this.isLoading.set(false);
        }
      },
      error: (error) => {
        console.error('Error cargando subasta:', error);
        if (!silent) {
          this.errorMessage.set('Error al cargar la subasta');
          this.isLoading.set(false);
        }
      }
    });
  }

  seleccionarImagen(index: number): void {
    this.imagenSeleccionada.set(index);    this.pausarAutoplay();
  }

  siguienteImagen() {
    const total = this.totalImagenes();
    if (total > 0) {
      this.imagenSeleccionada.update(i => (i + 1) % total);
    }
  }

  anteriorImagen() {
    const total = this.totalImagenes();
    if (total > 0) {
      this.imagenSeleccionada.update(i => (i - 1 + total) % total);
    }
  }

  iniciarAutoplay() {
    if (this.totalImagenes() > 1) {
      this.carouselInterval = setInterval(() => {
        if (this.autoplayEnabled()) {
          this.siguienteImagen();
        }
      }, 4000);
    }
  }

  pausarAutoplay() {
    this.autoplayEnabled.set(false);
    if (this.carouselInterval) {
      clearInterval(this.carouselInterval);
    }
    // Reanudar después de 10 segundos de inactividad
    setTimeout(() => {
      this.autoplayEnabled.set(true);
      this.iniciarAutoplay();
    }, 10000);
  }

  detenerAutoplay() {
    this.autoplayEnabled.set(false);
    if (this.carouselInterval) {
      clearInterval(this.carouselInterval);
    }  }

  incrementarPuja(): void {
    const sub = this.subasta();
    if (sub) {
      this.cantidadPuja.set(this.cantidadPuja() + sub.incrementoMinimo);
    }
  }

  realizarPuja(): void {
    const sub = this.subasta();
    const user = this.currentUser();
    
    if (!sub || !user) {
      this.toast.error('No hay sesión de usuario o subasta');
      return;
    }

    // Validar que el usuario esté validado
    if (user.validado !== 1 && user.rol?.toLowerCase() !== 'administrador') {
      if (!user.documentoIAE) {
        this.toast.error('Debes subir tu documento IAE antes de poder pujar');
        this.router.navigate(['/subir-iae']);
        return;
      }
      this.toast.error('Tu cuenta debe estar validada para poder pujar. Espera la validación del administrador.');
      return;
    }

    // Validar que la puja sea mayor al precio actual
    if (!this.cantidadPuja() || this.cantidadPuja() <= sub.precioActual) {
      this.toast.error(`La puja debe ser mayor a ${sub.precioActual.toLocaleString()}€`);
      return;
    }
    
    // Validar incremento mínimo
    const incrementoRequerido = sub.precioActual + sub.incrementoMinimo;
    if (this.cantidadPuja() < incrementoRequerido) {
      this.toast.error(`La puja mínima debe ser de ${incrementoRequerido.toLocaleString()}€ (incremento mínimo: ${sub.incrementoMinimo.toLocaleString()}€)`);
      return;
    }

    this.isPujando.set(true);
    this.errorMessage.set(null);
    this.successMessage.set(null);

    const idUsuario = user.idUsuario || (user.id ? parseInt(user.id) : null);
    
    if (!idUsuario) {
      this.toast.error('Error: No se pudo identificar al usuario');
      this.isPujando.set(false);
      return;
    }

    const nuevaPuja = {
      idSubasta: sub.idSubasta,
      idUsuario: idUsuario,
      cantidad: this.cantidadPuja()
    };

    console.log('Datos de puja a enviar:', nuevaPuja);

    const cantidadPujada = this.cantidadPuja();

    this.http.post(getApiUrl('/api/Pujas'), nuevaPuja).subscribe({
      next: () => {
        this.toast.success(`¡Puja realizada! Has pujado ${cantidadPujada.toLocaleString()}€`);
        this.isPujando.set(false);
        // Recargar subasta para actualizar precio (modo silencioso)
        this.cargarSubasta(sub.idSubasta, true);
      },
      error: (error) => {
        console.error('Error realizando puja:', error);
        console.error('Detalles del error:', error.error);
        
        // Manejar errores específicos de validación
        if (error.error?.requiereDocumento) {
          this.toast.error(error.error.mensaje);
          this.router.navigate(['/subir-iae']);
          this.isPujando.set(false);
          return;
        }
        
        if (error.error?.requiereValidacion) {
          this.toast.error(error.error.mensaje);
          this.isPujando.set(false);
          return;
        }
        
        // Si es error 500, probablemente la puja se insertó pero falló la serialización
        if (error.status === 500 && error.error?.includes('object cycle')) {
          this.toast.success(`¡Puja realizada! Has pujado ${cantidadPujada.toLocaleString()}€`);
          this.isPujando.set(false);
          // Forzar recarga para verificar que se insertó
          setTimeout(() => this.cargarSubasta(sub.idSubasta, true), 1000);
        } else {
          const mensaje = error.error?.message || error.error || 'Error al realizar la puja';
          this.toast.error(mensaje);
          this.isPujando.set(false);
        }
      }
    });
  }

  volver(): void {
    this.router.navigate(['/subastas']);
  }

  getImagenUrl(imagen: any): string {
    if (imagen?.imagenBase64) {
      return `data:image/jpeg;base64,${imagen.imagenBase64}`;
    }
    
    if (imagen?.ruta) {
      // Si es URL completa, usarla tal cual
      if (imagen.ruta.startsWith('http')) {
        return imagen.ruta;
      }
      // Si no, construir URL apuntando al backend
      const rutaLimpia = imagen.ruta.startsWith('/img/') ? imagen.ruta.substring(5) : imagen.ruta;
      const baseUrl = window.location.hostname === 'localhost' 
        ? 'http://localhost:56801' 
        : 'https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net';
      return `${baseUrl}/img/${rutaLimpia}`;
    }
    
    return '/assets/images/no-image.svg';
  }

  async onFileSelected(event: any): Promise<void> {
    const files: FileList = event.target.files;
    if (!files || files.length === 0) return;

    const sub = this.subasta();
    if (!sub) return;

    // Convertir archivos a base64
    const imagenesPromesas = Array.from(files).map(file => this.convertirArchivoABase64(file));
    const imagenesBase64Array = await Promise.all(imagenesPromesas);
    
    const nuevasImagenes = imagenesBase64Array.map((base64, index) => ({
      nombreImagen: files[index].name,
      esActivoImagen: true,
      imagenBase64: base64
    }));

    // Enviar imágenes como JSON
    this.http.post(getApiUrl(`/api/Vehiculos/${sub.idVehiculo}/imagenes`), { imagenes: nuevasImagenes }).subscribe({
      next: () => {
        this.successMessage.set('Imágenes añadidas correctamente');
        setTimeout(() => this.successMessage.set(null), 3000);
        this.cargarSubasta(sub.idSubasta, true);
      },
      error: (error) => {
        console.error('Error añadiendo imágenes:', error);
        this.errorMessage.set('Error al añadir las imágenes');
        setTimeout(() => this.errorMessage.set(null), 3000);
      }
    });
  }

  private convertirArchivoABase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const base64 = reader.result as string;
        const base64String = base64.split(',')[1];
        resolve(base64String);
      };
      reader.onerror = error => reject(error);
      reader.readAsDataURL(file);
    });
  }

  eliminarImagen(index: number): void {
    const sub = this.subasta();
    if (!sub || !sub.vehiculo?.imagenes) return;

    const imagen = sub.vehiculo.imagenes[index];
    if (!confirm('¿Estás seguro de eliminar esta imagen?')) return;

    this.http.delete(getApiUrl(`/api/Vehiculos/${sub.idVehiculo}/imagenes/${imagen.idImagen}`)).subscribe({
      next: () => {
        this.successMessage.set('Imagen eliminada correctamente');
        setTimeout(() => this.successMessage.set(null), 3000);
        
        // Si era la imagen seleccionada, volver a la primera
        if (this.imagenSeleccionada() >= index && this.imagenSeleccionada() > 0) {
          this.imagenSeleccionada.set(this.imagenSeleccionada() - 1);
        }
        
        this.cargarSubasta(sub.idSubasta, true);
      },
      error: (error) => {
        console.error('Error eliminando imagen:', error);
        this.errorMessage.set('Error al eliminar la imagen');
        setTimeout(() => this.errorMessage.set(null), 3000);
      }
    });
  }

  private procesarSubastaFinalizada(): void {
    // Solo procesar una vez
    const sub = this.subasta();
    if (!sub || sub.estado !== 'activa') return;

    console.log('Procesando subasta finalizada:', sub.idSubasta);

    // Llamar al endpoint para procesar la subasta finalizada
    this.http.post(getApiUrl('/api/Notificaciones/procesar-finalizadas'), {}).subscribe({
      next: () => {
        console.log('Subasta finalizada procesada correctamente');
        // Actualizar estado local para evitar procesar múltiples veces
        this.subasta.update(s => s ? { ...s, estado: 'finalizada' } : null);
        
        // Mostrar mensaje al usuario
        this.successMessage.set('¡Subasta finalizada! Las notificaciones han sido enviadas a todos los participantes.');
        
        // Recargar la subasta después de 2 segundos
        setTimeout(() => {
          this.cargarSubasta(sub.idSubasta, true);
        }, 2000);
      },
      error: (error) => {
        console.error('Error procesando subasta finalizada:', error);
      }
    });
  }
}
