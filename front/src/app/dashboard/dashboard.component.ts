import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
import { NotificationService } from '../services/notification.service';
import { getApiUrl } from '../utils/api-url.helper';

interface Puja {
  idPuja: number;
  idSubasta: number;
  esGanadora?: boolean;
  subasta?: {
    fechaFin: string;
  };
}

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private notificationService = inject(NotificationService);
  private http = inject(HttpClient);
  private router = inject(Router);

  currentUser = this.authService.currentUser;
  procesandoNotificaciones = signal(false);
  mensajeProceso = signal<string | null>(null);
  
  subastasGanadas = signal(0);
  subastasParticipadas = signal(0);
  proximasAFinalizar = signal(0);
  
  // Listas completas para mostrar detalles
  pujasGanadas = signal<Puja[]>([]);
  pujasParticipadas = signal<Puja[]>([]);
  subastasUnicasParticipadas = signal<{idSubasta: number, totalPujas: number}[]>([]);
  subastasProximas = signal<any[]>([]);
  pujasUsuarioIds = signal<number[]>([]);

  esAdmin = computed(() => {
    const user = this.currentUser();
    return user?.rol?.toLowerCase() === 'administrador';
  });

  ngOnInit() {
    this.cargarEstadisticas();
    // Recargar cada 30 segundos
    setInterval(() => this.cargarEstadisticas(), 30000);
  }

  cargarEstadisticas() {
    const user = this.currentUser();
    if (!user?.idUsuario) return;

    // Cargar pujas del usuario
    this.http.get<Puja[]>(getApiUrl(`/api/Pujas/usuario/${user.idUsuario}`)).subscribe({
      next: (pujas) => {
        // Contar subastas ganadas y guardar lista
        const pujasGanadasList = pujas.filter(p => p.esGanadora === true);
        this.subastasGanadas.set(pujasGanadasList.length);
        this.pujasGanadas.set(pujasGanadasList);
        
        // Contar subastas participadas (únicas por idSubasta)
        const subastasMap = new Map<number, number>();
        pujas.forEach(p => {
          subastasMap.set(p.idSubasta, (subastasMap.get(p.idSubasta) || 0) + 1);
        });
        
        const subastasUnicas = Array.from(subastasMap.entries()).map(([idSubasta, totalPujas]) => ({
          idSubasta,
          totalPujas
        }));
        
        this.subastasParticipadas.set(subastasUnicas.length);
        this.subastasUnicasParticipadas.set(subastasUnicas);
        this.pujasParticipadas.set(pujas);
        
        // Guardar IDs de subastas donde ha pujado
        const idsSubastas = pujas.map(p => p.idSubasta);
        this.pujasUsuarioIds.set(idsSubastas);
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error cargando pujas:', error);
        }
      }
    });

    // Cargar subastas activas próximas a finalizar (menos de 24 horas)
    this.http.get<any[]>(getApiUrl('/api/Subastas?activas=true')).subscribe({
      next: (subastas) => {
        const ahora = new Date().getTime();
        const proximasAfinalizar = subastas.filter(s => {
          const fechaFin = new Date(s.fechaFin).getTime();
          const diff = fechaFin - ahora;
          return diff > 0 && diff < 24 * 60 * 60 * 1000; // Menos de 24 horas
        });
        this.proximasAFinalizar.set(proximasAfinalizar.length);
        this.subastasProximas.set(proximasAfinalizar);
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error cargando subastas:', error);
        }
      }
    });
  }

  procesarSubastasFinalizadas() {
    this.procesandoNotificaciones.set(true);
    this.mensajeProceso.set(null);

    this.notificationService.procesarSubastasFinalizadas().subscribe({
      next: () => {
        this.mensajeProceso.set('Notificaciones procesadas correctamente');
        setTimeout(() => this.mensajeProceso.set(null), 3000);
        this.procesandoNotificaciones.set(false);
      },
      error: (error) => {
        console.error('Error procesando notificaciones:', error);
        this.mensajeProceso.set('Error al procesar notificaciones');
        setTimeout(() => this.mensajeProceso.set(null), 3000);
        this.procesandoNotificaciones.set(false);
      }
    });
  }
  
  verSubasta(idSubasta: number): void {
    this.router.navigate(['/subastas', idSubasta]);
  }
  
  usuarioHaPujado(idSubasta: number): boolean {
    return this.pujasUsuarioIds().includes(idSubasta);
  }
}
