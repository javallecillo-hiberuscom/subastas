import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { Empresa } from '../../models/app.models';

@Component({
  selector: 'app-empresas-admin',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './empresas-admin.component.html',
  styleUrl: './empresas-admin.component.css'
})
export class EmpresasAdminComponent implements OnInit {
  private http = inject(HttpClient);
  private fb = inject(FormBuilder);
  
  empresas = signal<Empresa[]>([]);
  loading = signal(true);
  showForm = signal(false);
  empresaForm!: FormGroup;
  editingId: number | null = null;
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);

  ngOnInit() {
    this.inicializarFormulario();
    this.cargarEmpresas();
  }

  inicializarFormulario() {
    this.empresaForm = this.fb.group({
      cif: ['', [Validators.required, Validators.pattern(/^[A-Z][0-9]{8}$/)]],
      nombre: ['', [Validators.required, Validators.minLength(3)]],
      direccion: ['', Validators.required],
      telefono: ['', Validators.pattern(/^[0-9]{9}$/)],
      email: ['', [Validators.email]],
      activo: [true]
    });
  }

  cargarEmpresas() {
    this.loading.set(true);
    this.http.get<Empresa[]>('/api/Empresas').subscribe({
      next: (empresas) => {
        this.empresas.set(empresas);
        this.loading.set(false);
      },
      error: (error) => {
        console.error('Error cargando empresas:', error);
        this.errorMessage.set('Error al cargar las empresas');
        this.loading.set(false);
      }
    });
  }

  mostrarFormulario() {
    this.showForm.set(true);
    this.editingId = null;
    this.empresaForm.reset({ activo: true });
  }

  editarEmpresa(empresa: Empresa) {
    this.editingId = empresa.idEmpresa;
    this.empresaForm.patchValue(empresa);
    this.showForm.set(true);
  }

  cancelar() {
    this.showForm.set(false);
    this.editingId = null;
    this.empresaForm.reset();
  }

  guardar() {
    if (this.empresaForm.invalid) {
      this.errorMessage.set('Por favor, completa todos los campos correctamente');
      setTimeout(() => this.errorMessage.set(null), 3000);
      return;
    }

    const empresaData = this.empresaForm.value;

    if (this.editingId) {
      // Actualizar
      this.http.put(`/api/Empresas/${this.editingId}`, { ...empresaData, idEmpresa: this.editingId }).subscribe({
        next: () => {
          this.successMessage.set('Empresa actualizada correctamente');
          setTimeout(() => this.successMessage.set(null), 3000);
          this.cargarEmpresas();
          this.cancelar();
        },
        error: (error) => {
          console.error('Error actualizando empresa:', error);
          this.errorMessage.set('Error al actualizar la empresa');
          setTimeout(() => this.errorMessage.set(null), 3000);
        }
      });
    } else {
      // Crear
      this.http.post('/api/Empresas', empresaData).subscribe({
        next: () => {
          this.successMessage.set('Empresa creada correctamente');
          setTimeout(() => this.successMessage.set(null), 3000);
          this.cargarEmpresas();
          this.cancelar();
        },
        error: (error) => {
          console.error('Error creando empresa:', error);
          this.errorMessage.set('Error al crear la empresa');
          setTimeout(() => this.errorMessage.set(null), 3000);
        }
      });
    }
  }

  eliminar(id: number) {
    if (!confirm('¿Estás seguro de que quieres eliminar esta empresa?')) {
      return;
    }

    this.http.delete(`/api/Empresas/${id}`).subscribe({
      next: () => {
        this.successMessage.set('Empresa eliminada correctamente');
        setTimeout(() => this.successMessage.set(null), 3000);
        this.cargarEmpresas();
      },
      error: (error) => {
        console.error('Error eliminando empresa:', error);
        this.errorMessage.set('Error al eliminar la empresa');
        setTimeout(() => this.errorMessage.set(null), 3000);
      }
    });
  }

  toggleActivo(empresa: Empresa) {
    const empresaActualizada = { ...empresa, activo: !empresa.activo };
    this.http.put(`/api/Empresas/${empresa.idEmpresa}`, empresaActualizada).subscribe({
      next: () => {
        this.successMessage.set('Estado actualizado correctamente');
        setTimeout(() => this.successMessage.set(null), 3000);
        this.cargarEmpresas();
      },
      error: (error) => {
        console.error('Error actualizando estado:', error);
        this.errorMessage.set('Error al actualizar el estado');
        setTimeout(() => this.errorMessage.set(null), 3000);
      }
    });
  }
}
