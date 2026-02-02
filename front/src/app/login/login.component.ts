import { Component, signal, inject } from '@angular/core';
import { Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { LoginRequest } from '../models/auth.models';
import { getApiUrl } from '../utils/api-url.helper';

@Component({
  selector: 'app-login',
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css'
})
export class LoginComponent {
  private authService = inject(AuthService);
  private router = inject(Router);
  private http = inject(HttpClient);

  credentials = signal<LoginRequest>({
    Email: '',
    Password: ''
  });

  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  showPassword = signal(false);

  onSubmit(): void {
    if (!this.credentials().Email || !this.credentials().Password) {
      this.errorMessage.set('Por favor, complete todos los campos');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set(null);

    this.authService.login(this.credentials().Email, this.credentials().Password).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.router.navigate(['/']);
      },
      error: (error) => {
        this.isLoading.set(false);
        const mensaje = error.status === 401 
          ? 'Email o contraseña incorrectos'
          : 'Error al iniciar sesión. Por favor, intente nuevamente.';
        this.errorMessage.set(mensaje);
      }
    });
  }

  updateEmail(email: string): void {
    this.credentials.update(cred => ({ ...cred, Email: email }));
  }

  updatePassword(password: string): void {
    this.credentials.update(cred => ({ ...cred, Password: password }));
  }

  togglePasswordVisibility(): void {
    this.showPassword.update(show => !show);
  }

  recuperarContrasena(): void {
    const email = prompt('Ingresa tu correo electrónico:');
    
    if (!email || !email.trim()) {
      return;
    }

    // Validar formato de email básico
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      alert('Por favor, ingresa un correo electrónico válido.');
      return;
    }

    this.isLoading.set(true);

    this.http.post(getApiUrl('/api/Usuarios/recuperar-contrasena'), { Email: email }).subscribe({
      next: (response: any) => {
        this.isLoading.set(false);
        
        // Mostrar mensaje con el código (solo para desarrollo)
        const codigo = response.codigo;
        alert(`${response.mensaje}\n\n⚠️ Código de recuperación (solo desarrollo): ${codigo}`);
        
        // Solicitar código y nueva contraseña
        this.solicitarNuevaContrasena(email);
      },
      error: (error) => {
        this.isLoading.set(false);
        alert('Error al enviar el código de recuperación. Por favor, intenta nuevamente.');
        console.error('Error recuperación:', error);
      }
    });
  }

  private solicitarNuevaContrasena(email: string): void {
    const codigo = prompt('Ingresa el código de recuperación que recibiste:');
    
    if (!codigo || !codigo.trim()) {
      return;
    }

    const nuevaContrasena = prompt('Ingresa tu nueva contraseña:');
    
    if (!nuevaContrasena || nuevaContrasena.length < 6) {
      alert('La contraseña debe tener al menos 6 caracteres.');
      return;
    }

    const confirmarContrasena = prompt('Confirma tu nueva contraseña:');
    
    if (nuevaContrasena !== confirmarContrasena) {
      alert('Las contraseñas no coinciden.');
      return;
    }

    this.isLoading.set(true);

    this.http.post(getApiUrl('/api/Usuarios/restablecer-contrasena'), {
      Email: email,
      Codigo: codigo,
      NuevaContrasena: nuevaContrasena
    }).subscribe({
      next: (response: any) => {
        this.isLoading.set(false);
        alert(response.mensaje + '\n\nYa puedes iniciar sesión con tu nueva contraseña.');
        
        // Pre-llenar el email en el formulario
        this.credentials.update(cred => ({ ...cred, Email: email }));
      },
      error: (error) => {
        this.isLoading.set(false);
        const mensaje = error.error?.mensaje || 'Error al restablecer la contraseña. Verifica el código e intenta nuevamente.';
        alert(mensaje);
        console.error('Error restablecer:', error);
      }
    });
  }
}
