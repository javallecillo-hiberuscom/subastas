import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { getApiUrl } from '../utils/api-url.helper';

@Component({
  selector: 'app-registro',
  imports: [CommonModule, ReactiveFormsModule, RouterModule],
  templateUrl: './registro.component.html',
  styleUrl: './registro.component.css'
})
export class RegistroComponent {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private router = inject(Router);

  registroForm: FormGroup;
  documentoIAE: File | null = null;
  loading = signal(false);
  errorMessage = signal<string | null>(null);
  successMessage = signal<string | null>(null);
  showPassword = signal(false);

  constructor() {
    this.registroForm = this.fb.group({
      nombre: ['', [Validators.required, Validators.minLength(3)]],
      email: ['', [Validators.required, Validators.email]],
      dni: ['', [Validators.required, Validators.pattern(/^[0-9]{8}[A-Z]$/)]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      cif: ['', [Validators.required, Validators.pattern(/^[A-Z][0-9]{8}$/)]],
      direccionEmpresa: ['', Validators.required],
      telefonoContacto: ['', [Validators.required, Validators.pattern(/^[0-9]{9}$/)]],
      aceptaTerminos: [false, Validators.requiredTrue]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.documentoIAE = input.files[0];
    }
  }

  togglePasswordVisibility() {
    this.showPassword.update(v => !v);
  }

  onSubmit() {
    if (this.registroForm.valid) {
      this.loading.set(true);
      this.errorMessage.set(null);
      this.successMessage.set(null);

      const registroData = {
        Nombre: this.registroForm.get('nombre')?.value,
        Apellidos: this.registroForm.get('apellidos')?.value || '',
        Email: this.registroForm.get('email')?.value,
        Password: this.registroForm.get('password')?.value,
        Telefono: this.registroForm.get('telefonoContacto')?.value || null,
        Direccion: this.registroForm.get('direccionEmpresa')?.value || null,
        IdEmpresa: null
      };

      this.http.post(getApiUrl('/api/Usuarios/registro'), registroData).subscribe({
        next: () => {
          this.successMessage.set('Registro exitoso. Tu cuenta está pendiente de validación por un administrador.');
          this.loading.set(false);
          
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 3000);
        },
        error: (error) => {
          this.errorMessage.set(error.error?.Message || error.error?.message || 'Error al registrar usuario');
          this.loading.set(false);
        }
      });
    } else {
      Object.keys(this.registroForm.controls).forEach(key => {
        this.registroForm.get(key)?.markAsTouched();
      });
    }
  }
}
