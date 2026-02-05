import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { getApiUrl } from '../utils/api-url.helper';

@Component({
  selector: 'app-registro',
  standalone: true,
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
      nombre: ['', [Validators.required, Validators.minLength(2)]],
      apellidos: [''],
      email: ['', [Validators.required, Validators.email]],
      dni: ['', [this.dniValidator]],
      password: ['', [Validators.required, Validators.minLength(6)]],
      confirmPassword: ['', Validators.required],
      cif: [''],
      direccionEmpresa: [''],
      telefonoContacto: [''],
      aceptaTerminos: [false, Validators.requiredTrue]
    }, { validators: this.passwordMatchValidator });
  }

  passwordMatchValidator(g: FormGroup) {
    return g.get('password')?.value === g.get('confirmPassword')?.value
      ? null : { mismatch: true };
  }

  static validarDniNif(valor: string): boolean {
    if (!valor) return false;
    valor = valor.toUpperCase().replace(/\s|-/g, '');
    // DNI: 8 números y una letra
    if (/^\d{8}[A-Z]$/.test(valor)) {
      const letras = 'TRWAGMYFPDXBNJZSQVHLCKE';
      const numero = parseInt(valor.substring(0, 8), 10);
      const letra = valor.charAt(8);
      return letra === letras[numero % 23];
    }
    // NIF/CIF: otras validaciones posibles (simplificado)
    // Aquí puedes añadir lógica para NIE, CIF, etc.
    return false;
  }

  dniValidator(control: any) {
    const valor = control.value;
    if (!valor) return null;
    return RegistroComponent.validarDniNif(valor) ? null : { dniInvalido: true };
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
          this.successMessage.set('Registro exitoso. Redirigiendo al login...');
          this.loading.set(false);
          
          setTimeout(() => {
            this.router.navigate(['/login']);
          }, 1500);
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
