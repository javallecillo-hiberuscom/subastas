import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Vehiculo, TipoMotor, TipoCarroceria, ImagenVehiculo } from '../../models/app.models';
import { getApiUrl } from '../../utils/api-url.helper';

@Component({
  selector: 'app-vehiculos-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './vehiculos-admin.component.html',
  styleUrl: './vehiculos-admin.component.css'
})
export class VehiculosAdminComponent implements OnInit {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  
  vehiculos = signal<Vehiculo[]>([]);
  loading = signal(true);
  busqueda = signal('');
  showForm = signal(false);
  vehiculoForm!: FormGroup;
  editingId: number | null = null;
  selectedFiles: File[] = [];
  imagenesBase64 = signal<ImagenVehiculo[]>([]);
  isDragging = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  tiposMotor = Object.values(TipoMotor);
  tiposCarroceria = Object.values(TipoCarroceria);

  ngOnInit() {
    this.inicializarFormulario();
    this.cargarVehiculos();
  }

  inicializarFormulario() {
    this.vehiculoForm = this.fb.group({
      matricula: ['', [Validators.required, Validators.pattern(/^[0-9]{4}[A-Z]{3}$/)]],
      marca: ['', Validators.required],
      modelo: ['', Validators.required],
      numeroPuertas: [4, [Validators.required, Validators.min(2), Validators.max(5)]],
      kilometraje: [0, [Validators.required, Validators.min(0)]],
      valoracionPrecio: [0, [Validators.required, Validators.min(0)]],
      precioSalida: [0, [Validators.required, Validators.min(0)]],
      anio: [new Date().getFullYear(), [Validators.required, Validators.min(1900)]],
      color: ['', Validators.required],
      descripcion: [''],
      potencia: [0, [Validators.required, Validators.min(0)]],
      tipoCarroceria: [TipoCarroceria.Berlina, Validators.required],
      tipoMotor: [TipoMotor.Gasolina, Validators.required],
      fechaMatriculacion: ['', Validators.required]
    });
  }

  cargarVehiculos() {
    this.loading.set(true);
    this.http.get<Vehiculo[]>(getApiUrl('/api/Vehiculos')).subscribe({
      next: (vehiculos) => {
        this.vehiculos.set(vehiculos);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error cargando vehículos:', error);
        this.errorMessage.set('Error al cargar vehículos');
        this.loading.set(false);
      }
    });
  }

  vehiculosFiltrados() {
    const busquedaLower = this.busqueda().toLowerCase();
    if (!busquedaLower) return this.vehiculos();
    
    return this.vehiculos().filter(v => 
      v.marca?.toLowerCase().includes(busquedaLower) ||
      v.modelo?.toLowerCase().includes(busquedaLower) ||
      v.matricula?.toLowerCase().includes(busquedaLower) ||
      v.color?.toLowerCase().includes(busquedaLower)
    );
  }

  nuevoVehiculo() {
    this.editingId = null;
    this.selectedFiles = [];
    this.imagenesBase64.set([]);
    this.vehiculoForm.reset({
      numeroPuertas: 4,
      kilometraje: 0,
      valoracionPrecio: 0,
      precioSalida: 0,
      anio: new Date().getFullYear(),
      potencia: 0,
      tipoCarroceria: TipoCarroceria.Berlina,
      tipoMotor: TipoMotor.Gasolina
    });
    this.showForm.set(true);
  }

  editarVehiculo(vehiculo: Vehiculo) {
    this.editingId = vehiculo.idVehiculo;
    this.imagenesBase64.set(vehiculo.imagenes || []);
    
    // Convertir fechaMatriculacion solo si existe y es válida
    let fechaMatriculacion = null;
    if (vehiculo.fechaMatriculacion) {
      const fecha = new Date(vehiculo.fechaMatriculacion);
      if (!isNaN(fecha.getTime())) {
        fechaMatriculacion = fecha.toISOString().split('T')[0];
      }
    }
    
    this.vehiculoForm.patchValue({
      ...vehiculo,
      fechaMatriculacion: fechaMatriculacion
    });
    this.showForm.set(true);
  }

  guardarVehiculo() {
    if (this.vehiculoForm.invalid) {
      this.errorMessage.set('Por favor completa todos los campos correctamente');
      setTimeout(() => this.errorMessage.set(null), 3000);
      return;
    }

    const vehiculoData = {
      ...this.vehiculoForm.value,
      imagenes: this.imagenesBase64()
    };
    
    if (this.editingId) {
      // Actualizar vehículo existente
      this.http.put(getApiUrl(`/api/Vehiculos/${this.editingId}`), { ...vehiculoData, idVehiculo: this.editingId }).subscribe({
        next: () => {
          this.successMessage.set('Vehículo actualizado correctamente');
          setTimeout(() => this.successMessage.set(null), 3000);
          this.cargarVehiculos();
          this.cancelar();
        },
        error: (error) => {
          console.error('Error actualizando vehículo:', error);
          this.errorMessage.set('Error al actualizar el vehículo');
          setTimeout(() => this.errorMessage.set(null), 3000);
        }
      });
    } else {
      // Crear vehículo con imágenes incluidas
      this.http.post<Vehiculo>(getApiUrl('/api/Vehiculos'), vehiculoData).subscribe({
        next: () => {
          this.successMessage.set('Vehículo creado correctamente');
          setTimeout(() => this.successMessage.set(null), 3000);
          this.cargarVehiculos();
          this.cancelar();
        },
        error: (error) => {
          console.error('Error creando vehículo:', error);
          this.errorMessage.set('Error al crear el vehículo');
          setTimeout(() => this.errorMessage.set(null), 3000);
        }
      });
    }
  }

  async onFileSelected(event: any) {
    const files: FileList = event.target.files;
    await this.procesarArchivos(files);
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
      await this.procesarArchivos(files);
    }
  }

  private async procesarArchivos(files: FileList) {
    this.selectedFiles = Array.from(files);
    
    // Convertir archivos a base64
    const imagenesPromesas = Array.from(files).map(file => this.convertirArchivoABase64(file));
    const imagenesBase64Array = await Promise.all(imagenesPromesas);
    
    const nuevasImagenes: ImagenVehiculo[] = imagenesBase64Array.map((base64, index) => ({
      nombre: files[index].name,
      activo: true,
      imagenBase64: base64
    }));
    
    this.imagenesBase64.update(imagenes => [...imagenes, ...nuevasImagenes]);
  }

  private convertirArchivoABase64(file: File): Promise<string> {
    return new Promise((resolve, reject) => {
      const reader = new FileReader();
      reader.onload = () => {
        const base64 = reader.result as string;
        // Extraer solo la parte base64 sin el prefijo data:image/...;base64,
        const base64String = base64.split(',')[1];
        resolve(base64String);
      };
      reader.onerror = error => reject(error);
      reader.readAsDataURL(file);
    });
  }

  removeFile(index: number) {
    this.imagenesBase64.update(imagenes => {
      const nuevasImagenes = [...imagenes];
      nuevasImagenes.splice(index, 1);
      return nuevasImagenes;
    });
  }

  eliminarVehiculo(id: number) {
    if (!confirm('¿Estás seguro de eliminar este vehículo?')) return;

    this.http.delete(getApiUrl(`/api/Vehiculos/${id}`)).subscribe({
      next: () => {
        this.successMessage.set('Vehículo eliminado correctamente');
        setTimeout(() => this.successMessage.set(null), 3000);
        this.cargarVehiculos();
      },
      error: (error) => {
        console.error('Error eliminando vehículo:', error);
        this.errorMessage.set('Error al eliminar el vehículo');
        setTimeout(() => this.errorMessage.set(null), 3000);
      }
    });
  }

  cancelar() {
    this.showForm.set(false);
    this.editingId = null;
    this.selectedFiles = [];
    this.imagenesBase64.set([]);
    this.vehiculoForm.reset();
  }
}
