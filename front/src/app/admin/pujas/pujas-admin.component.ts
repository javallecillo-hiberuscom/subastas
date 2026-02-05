import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { PujaService, VehiculoService } from '../../services/vehiculo.service';
import { Puja, Vehiculo, Subasta } from '../../models/app.models';
import { HttpClient } from '@angular/common/http';
import { getApiUrl } from '../../utils/api-url.helper';

@Component({
  selector: 'app-pujas-admin',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './pujas-admin.component.html',
  styleUrl: './pujas-admin.component.css'
})
export class PujasAdminComponent implements OnInit {
  private pujaService = inject(PujaService);
  private vehiculoService = inject(VehiculoService);
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  
  pujas = signal<Puja[]>([]);
  subastas = signal<Subasta[]>([]);
  loading = signal(true);
  showForm = signal(false);
  pujaForm!: FormGroup;
  editingId: number | null = null;

  ngOnInit() {
    this.inicializarFormulario();
    this.cargarPujas();
    this.cargarSubastas();
  }

  inicializarFormulario() {
    this.pujaForm = this.fb.group({
      idSubasta: ['', Validators.required],
      idUsuario: ['', Validators.required],
      cantidad: [0, [Validators.required, Validators.min(0)]],
      fechaPuja: ['', Validators.required]
    });
  }

  cargarPujas() {
    this.pujaService.getPujas().subscribe({
      next: (pujas) => {
        console.log('Pujas recibidas del backend:', pujas);
        console.log('Cantidad de pujas:', pujas.length);
        this.pujas.set(pujas);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error cargando pujas:', error);
        this.loading.set(false);
      }
    });
  }

  cargarSubastas() {
    // Cargar TODAS las subastas (activas e inactivas) para mostrar información completa
    this.http.get<Subasta[]>(getApiUrl('/api/Subastas')).subscribe({
      next: (subastas) => {
        console.log('Subastas cargadas:', subastas.length);
        this.subastas.set(subastas);
      },
      error: (error) => {
        console.error('Error cargando subastas:', error);
        // Si falla, intentar solo activas
        this.http.get<Subasta[]>(getApiUrl('/api/Subastas?activas=true')).subscribe({
          next: (subastas) => this.subastas.set(subastas),
          error: (e) => console.error('Error cargando subastas activas:', e)
        });
      }
    });
  }

  nuevaPuja() {
    this.editingId = null;
    this.pujaForm.reset({
      cantidad: 0
    });
    this.showForm.set(true);
  }

  editarPuja(puja: Puja) {
    this.editingId = puja.idPuja;
    this.pujaForm.patchValue({
      idSubasta: puja.idSubasta,
      idUsuario: puja.idUsuario,
      cantidad: puja.cantidad,
      fechaPuja: new Date(puja.fechaPuja).toISOString().slice(0, 16)
    });
    this.showForm.set(true);
  }

  guardarPuja() {
    if (this.pujaForm.valid) {
      const pujaData = this.pujaForm.value;
      
      if (this.editingId) {
        this.pujaService.updatePuja(this.editingId, pujaData).subscribe({
          next: () => {
            this.cargarPujas();
            this.cancelar();
          },
          error: (error) => console.error('Error actualizando puja:', error)
        });
      } else {
        this.pujaService.createPuja(pujaData).subscribe({
          next: () => {
            this.cargarPujas();
            this.cancelar();
          },
          error: (error) => console.error('Error creando puja:', error)
        });
      }
    }
  }

  eliminarPuja(id: number) {
    if (confirm('¿Estás seguro de eliminar esta puja?')) {
      this.pujaService.deletePuja(id).subscribe({
        next: () => this.cargarPujas(),
        error: (error) => console.error('Error eliminando puja:', error)
      });
    }
  }

  cancelar() {
    this.showForm.set(false);
    this.editingId = null;
    this.pujaForm.reset();
  }

  getEstadoSubasta(puja: Puja): string {
    // Buscar la subasta en el listado si no viene en la puja
    const subasta = puja.subasta || this.subastas().find(s => s.idSubasta === puja.idSubasta);
    if (!subasta) return 'Desconocido';
    return new Date(subasta.fechaFin) > new Date() ? 'Activa' : 'Finalizada';
  }

  getSubastaNombre(idSubasta: number): string {
    const subasta = this.subastas().find(s => s.idSubasta === idSubasta);
    if (!subasta?.vehiculo) return `Subasta #${idSubasta}`;
    return `${subasta.vehiculo.marca} ${subasta.vehiculo.modelo}`;
  }

  getUsuarioNombre(puja: Puja): string {
    if (puja.usuario) {
      return `${puja.usuario.nombre} ${puja.usuario.apellidos || ''}`;
    }
    return puja.idUsuario ? `Usuario #${puja.idUsuario}` : 'Desconocido';
  }
}
