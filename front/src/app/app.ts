import { Component, signal, inject, OnInit } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AuthService } from './services/auth.service';
import { NotificationService } from './services/notification.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  protected readonly title = signal('front');
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);

  ngOnInit() {
    console.error('🔥🔥🔥 APP COMPONENT INIT');
    
    // Verificar si hay usuario autenticado y iniciar monitoreo
    const user = this.authService.currentUser();
    console.error('🔥 App: Usuario:', user);
    
    if (user) {
      const userId = user?.idUsuario || (user?.id ? parseInt(user.id) : null);
      const rolLower = user?.rol?.toLowerCase() || '';
      const esAdmin = rolLower === 'administrador' || rolLower === 'admin';
      
      console.error('🔥 App: Iniciando notificaciones - userId:', userId, 'esAdmin:', esAdmin);
      
      if (userId) {
        this.notificationService.iniciarMonitoreo(userId, esAdmin);
      }
    }
  }
}
