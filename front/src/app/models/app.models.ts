import { User } from './auth.models';

// Enumeraciones
export enum TipoMotor {
  Gasolina = 'Gasolina',
  Gasoil = 'Gasoil',
  Electrica = 'Eléctrica'
}

export enum TipoCarroceria {
  Berlina = 'Berlina',
  Compacto = 'Compacto',
  Todoterreno = 'Todoterreno',
  Deportivo = 'Deportivo',
  Descapotable = 'Descapotable',
  Familiar = 'Familiar',
  Coupe = 'Coupé'
}

export enum EstadoUsuario {
  NoRegistrado = 'NoRegistrado',
  Registrado = 'Registrado',
  Validado = 'Validado'
}

// Interfaces
export interface Vehiculo {
  idVehiculo: number;
  matricula: string;
  marca: string;
  modelo: string;
  numeroPuertas: number;
  kilometraje: number;
  valoracionPrecio: number;
  precioSalida: number;
  anio: number;
  color: string;
  descripcion: string;
  potencia: number;
  tipoCarroceria: TipoCarroceria;
  tipoMotor: TipoMotor;
  estaVendido: boolean;
  imagenes: ImagenVehiculo[];
  fechaCreacion: Date;
  fechaMatriculacion: Date;
  pujaActual?: Puja;
}

export interface ImagenVehiculo {
  idImagen?: number;
  nombre: string;
  activo: boolean;
  ruta?: string;
  imagenBase64?: string;
}

export interface Puja {
  idPuja: number;
  idSubasta: number;
  idUsuario?: number;
  cantidad: number;
  fechaPuja: Date;
  esGanadora?: boolean;
  subasta?: Subasta;
  usuario?: User;
}

export interface Subasta {
  idSubasta: number;
  idVehiculo: number;
  fechaInicio: Date;
  fechaFin: Date;
  precioInicial: number;
  precioActual: number;
  incrementoMinimo: number;
  estado: string;
  vehiculo?: Vehiculo;
}

export interface UsuarioDetalle {
  idUsuario: number;
  cif: string;
  direccionEmpresa: string;
  nombre: string;
  apellidos: string;
  esAdministrador: boolean;
  email: string;
  login: string;
  estaActivo: boolean;
  estaValidado: boolean;
  documentoIAE?: string;
}

export interface RegistroRequest {
  cif: string;
  direccionEmpresa: string;
  nombre: string;
  apellidos: string;
  email: string;
  login: string;
  password: string;
  documentoIAE?: File;
}

export interface PujaRequest {
  idPuja: number;
  cantidad: number;
}

export interface Empresa {
  idEmpresa: number;
  cif: string;
  nombre: string;
  direccion: string;
  telefono?: string;
  email?: string;
  activo: boolean;
  fechaCreacion?: Date;
}
