import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  registerForm: FormGroup;

  constructor(private fb: FormBuilder, private authService: AuthService) {
    this.registerForm = this.fb.group({
      nombre: ['', [Validators.required, Validators.maxLength(100)]],
      apellidos: ['', Validators.maxLength(100)],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(100)]],
      password: ['', [Validators.required, Validators.minLength(6), Validators.maxLength(255)]],
      dni: ['', [Validators.required, Validators.pattern(/^\d{8}[A-Z]$/), Validators.minLength(9), Validators.maxLength(9)]],
      telefono: ['', Validators.maxLength(20)],
      direccion: ['', Validators.maxLength(255)],
      idEmpresa: [null]
    });
  }

  onSubmit() {
    if (this.registerForm.valid) {
      this.authService.register(this.registerForm.value).subscribe({
        next: () => alert('Registro exitoso. Revisa tu email.'),
        error: (err) => alert('Error: ' + err.message)
      });
    } else {
      // Anunciar errores para lectores de pantalla
      const errors = this.getFormValidationErrors();
      alert('Errores en el formulario: ' + errors.join(', '));
    }
  }

  private getFormValidationErrors(): string[] {
    const errors: string[] = [];
    Object.keys(this.registerForm.controls).forEach(key => {
      const control = this.registerForm.get(key);
      if (control?.invalid) {
        errors.push(`${key}: ${Object.keys(control.errors || {}).join(', ')}`);
      }
    });
    return errors;
  }
}