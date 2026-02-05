import { Component, inject, OnInit, OnDestroy, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterModule, ActivatedRoute } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../services/auth.service';
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
  selector: 'app-lista-pujas',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './lista-pujas.component.html',
  styleUrl: './lista-pujas.component.css'
})
export class ListaPujasComponent implements OnInit, OnDestroy {
  private http = inject(HttpClient);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  private authService = inject(AuthService);

  subastas = signal<Subasta[]>([]);
  isLoading = signal(true);
  errorMessage = signal<string | null>(null);
  filtro = signal<'activas' | 'vencidas'>('activas');
  pujasUsuario = signal<number[]>([]);
  
  private countdownInterval?: number;
  
  currentUser = this.authService.currentUser;

  ngOnInit(): void {
    // Suscribirse a cambios en query params
    this.route.queryParams.subscribe(params => {
      const filtroParam = params['filtro'];
      if (filtroParam === 'activas' || filtroParam === 'vencidas') {
        this.filtro.set(filtroParam);
      }
      this.cargarSubastas();
    });
    
    // Cargar pujas del usuario
    const user = this.currentUser();
    if (user?.idUsuario) {
      this.cargarPujasUsuario(user.idUsuario);
    }
    
    // Actualizar countdown cada segundo
    this.countdownInterval = window.setInterval(() => {
      // Force update for countdown
      const current = this.subastas();
      this.subastas.set([...current]);
    }, 1000);
  }
  
  ngOnDestroy(): void {
    if (this.countdownInterval) {
      clearInterval(this.countdownInterval);
    }
  }

  cargarSubastas(): void {
    this.isLoading.set(true);
    const filtroActual = this.filtro();
    const url = filtroActual === 'activas' 
      ? getApiUrl('/api/Subastas?activas=true')
      : getApiUrl('/api/Subastas?activas=false');
    
    this.http.get<Subasta[]>(url).subscribe({
      next: (subastas) => {
        this.subastas.set(subastas);
        this.isLoading.set(false);
      },
      error: (error) => {
        console.error('Error cargando subastas:', error);
        this.errorMessage.set(`Error al cargar las subastas ${filtroActual}`);
        this.isLoading.set(false);
      }
    });
  }
  
  cargarPujasUsuario(idUsuario: number): void {
    this.http.get<any[]>(getApiUrl(`/api/Pujas/usuario/${idUsuario}`)).subscribe({
      next: (pujas) => {
        const idsSubastas = pujas.map(p => p.idSubasta);
        this.pujasUsuario.set(idsSubastas);
      },
      error: (error) => {
        if (error.status !== 404) {
          console.error('Error cargando pujas del usuario:', error);
        }
      }
    });
  }
  
  usuarioHaPujado(idSubasta: number): boolean {
    return this.pujasUsuario().includes(idSubasta);
  }

  verDetalle(idSubasta: number): void {
    if (!idSubasta) {
      console.error('idSubasta es undefined');
      return;
    }
    this.router.navigate(['/subastas', idSubasta]);
  }

  getTiempoRestante(fechaFin: string): string {
    const ahora = new Date();
    const fin = new Date(fechaFin);
    const diff = fin.getTime() - ahora.getTime();

    if (diff <= 0) return 'Finalizada';

    const dias = Math.floor(diff / (1000 * 60 * 60 * 24));
    const horas = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    const minutos = Math.floor((diff % (1000 * 60 * 60)) / (1000 * 60));
    const segundos = Math.floor((diff % (1000 * 60)) / 1000);

    if (dias > 0) return `${dias}d ${horas}h ${minutos}m`;
    if (horas > 0) return `${horas}h ${minutos}m ${segundos}s`;
    return `${minutos}m ${segundos}s`;
  }
  
  getEstadoSubasta(subasta: Subasta): string {
    const ahora = new Date();
    const inicio = new Date(subasta.fechaInicio);
    const fin = new Date(subasta.fechaFin);
    
    if (ahora < inicio) return 'Próximamente';
    if (ahora > fin) return 'Finalizada';
    return 'En subasta';
  }

  getImagenPrincipal(vehiculo: any): string {
    if (!vehiculo) return '/assets/images/no-image.svg';
    
    const imagen = vehiculo?.imagenes?.find((img: any) => img.activo) || vehiculo?.imagenes?.[0];
    
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
}
