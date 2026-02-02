import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { VehiculoService, PujaService } from '../services/vehiculo.service';
import { Vehiculo, Puja } from '../models/app.models';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-mis-pujas',
  imports: [CommonModule, RouterModule],
  templateUrl: './mis-pujas.component.html',
  styleUrl: './mis-pujas.component.css'
})
export class MisPujasComponent implements OnInit {
  private pujaService = inject(PujaService);
  private authService = inject(AuthService);
  private http = inject(HttpClient);
  
  misPujas = signal<Puja[]>([]);
  loading = signal(true);
  currentUser = this.authService.currentUser;
  filtro = signal<'activas' | 'superadas'>('activas');

  // Agrupa pujas por subasta y devuelve solo la mejor puja de cada subasta
  pujasAgrupadasPorSubasta = computed(() => {
    const pujas = this.misPujas();
    const mapa = new Map<number, Puja>();
    
    pujas.forEach(puja => {
      const idSubasta = puja.idSubasta;
      const pujaExistente = mapa.get(idSubasta);
      
      // Si no existe o esta puja es mayor, actualizar
      if (!pujaExistente || puja.cantidad > pujaExistente.cantidad) {
        mapa.set(idSubasta, puja);
      }
    });
    
    return Array.from(mapa.values());
  });

  pujasFiltradas = computed(() => {
    const pujas = this.pujasAgrupadasPorSubasta();
    const filtroActual = this.filtro();
    const ahora = new Date();
    
    return pujas.filter(puja => {
      if (!puja.subasta) return false;
      
      const fin = new Date(puja.subasta.fechaFin);
      const estaActiva = fin.getTime() > ahora.getTime();
      const estoyGanando = puja.subasta.precioActual && puja.cantidad >= puja.subasta.precioActual;
      
      if (filtroActual === 'activas') {
        // Activas: subastas en curso donde tengo la mejor puja
        return estaActiva && estoyGanando;
      } else {
        // Superadas: subastas donde fui superado (activas o finalizadas)
        return !estoyGanando;
      }
    });
  });

  ngOnInit() {
    this.cargarMisPujas();
  }

  cargarMisPujas() {
    const user = this.currentUser();
    
    if (user?.idUsuario) {
      this.http.get<Puja[]>(`/api/Pujas/usuario/${user.idUsuario}`).subscribe({
        next: (pujas) => {
          this.misPujas.set(pujas);
          this.loading.set(false);
        },
        error: (error) => {
          console.error('Error cargando pujas:', error);
          // Si es 404, significa que no tiene pujas
          if (error.status === 404) {
            this.misPujas.set([]);
          }
          this.loading.set(false);
        }
      });
    } else {
      this.loading.set(false);
    }
  }

  getTiempoRestante(puja: Puja): string {
    if (!puja.subasta) return 'Sin información';
    
    const ahora = new Date();
    const fin = new Date(puja.subasta.fechaFin);
    const diff = fin.getTime() - ahora.getTime();
    
    if (diff <= 0) return 'Finalizada';
    
    const dias = Math.floor(diff / (1000 * 60 * 60 * 24));
    const horas = Math.floor((diff % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60));
    
    if (dias > 0) return `${dias}d ${horas}h`;
    return `${horas}h`;
  }

  getImagenPrincipal(vehiculo: Vehiculo): string {
    // Si el vehículo no tiene imágenes, devolver placeholder
    if (!vehiculo?.imagenes || vehiculo.imagenes.length === 0) {
      return '/assets/no-image.jpg';
    }
    
    const imagenActiva = vehiculo.imagenes.find(img => img.activo);
    const imagen = imagenActiva || vehiculo.imagenes[0];
    
    // Si la ruta existe, construir la URL completa apuntando al backend
    if (imagen?.ruta) {
      return `https://localhost:7249${imagen.ruta}`;
    }
    
    return '/assets/no-image.jpg';
  }

  getEstadoPuja(puja: Puja): string {
    if (!puja.subasta) return 'Desconocido';
    
    const ahora = new Date();
    const fin = new Date(puja.subasta.fechaFin);
    const estaFinalizada = fin.getTime() <= ahora.getTime();
    
    // Si la subasta ya finalizó
    if (estaFinalizada) {
      // Comprobar si ganó (su puja es igual al precio actual final)
      if (puja.subasta.precioActual && puja.cantidad >= puja.subasta.precioActual) {
        return 'Ganada';
      }
      return 'Finalizada';
    }
    
    // Subasta activa: verificar si está ganando
    if (puja.subasta.precioActual && puja.cantidad >= puja.subasta.precioActual) {
      return 'Ganando';
    }
    
    return 'Superada';
  }
}
