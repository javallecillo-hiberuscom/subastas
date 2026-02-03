import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { getApiUrl } from '../utils/api-url.helper';

export interface EstadisticasGenerales {
  totalUsuarios: number;
  usuariosPendientes: number;
  usuariosValidados: number;
  totalEmpresas: number;
  totalVehiculos: number;
  totalSubastas: number;
  subastasActivas: number;
  subastasTerminadas: number;
}

export interface VehiculoInfo {
  marca: string;
  modelo: string;
  anio: number;
  matricula: string;
}

export interface UsuarioGanador {
  idUsuario: number;
  nombre: string;
  apellidos: string;
  email: string;
  telefono: string;
  empresa?: {
    nombre: string;
    cif: string;
  };
}

export interface PujaGanadora {
  cantidad: number;
  usuario: UsuarioGanador;
  fechaPuja: Date;
}

export interface SubastaActiva {
  idSubasta: number;
  fechaInicio: Date;
  fechaFin: Date;
  precioInicial: number;
  precioActual: number;
  tiempoRestante: string;
  vehiculo: VehiculoInfo;
  pujaGanadora: PujaGanadora | null;
  totalPujas: number;
}

export interface SubastaTerminada {
  idSubasta: number;
  fechaInicio: Date;
  fechaFin: Date;
  precioInicial: number;
  precioActual: number;
  tiempoFinalizada: string;
  vehiculo: VehiculoInfo;
  ganador: {
    pujaGanadora: number;
    usuario: UsuarioGanador;
    fechaPuja: Date;
  } | null;
  totalPujas: number;
  sinPujas: boolean;
}

export interface SubastaConPujas {
  idSubasta: number;
  vehiculo: {
    marca: string;
    modelo: string;
    anio: number;
  };
  totalPujas: number;
  estado: string;
}

export interface DashboardAdmin {
  estadisticasGenerales: EstadisticasGenerales;
  subastasActivas: SubastaActiva[];
  subastasTerminadas: SubastaTerminada[];
  todasSubastas: SubastaConPujas[];
}

export interface UsuarioPendiente {
  IdUsuario: number;
  Nombre: string;
  Apellidos: string;
  Email: string;
  Telefono: string;
  Direccion: string;
  DocumentoIAE: string;
  tieneDocumento: boolean;
  empresa: {
    NombreComercial: string;
    RazonSocial: string;
    Cif: string;
  } | null;
}

@Injectable({
  providedIn: 'root'
})
export class AdminService {
  constructor(private http: HttpClient) { }

  getDashboard(): Observable<DashboardAdmin> {
    return this.http.get<DashboardAdmin>(getApiUrl('/api/Admin/dashboard'));
  }

  getUsuariosPendientes(): Observable<UsuarioPendiente[]> {
    return this.http.get<UsuarioPendiente[]>(getApiUrl('/api/Admin/usuarios-pendientes'));
  }
}
