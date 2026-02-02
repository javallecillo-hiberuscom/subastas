export interface LoginRequest {
  Email: string;  // Backend espera Email con mayúscula
  Password: string;  // Backend espera Password con mayúscula
}

export interface LoginResponse {
  Token: string;
  IdUsuario: number;
  Email: string;
  NombreCompleto: string;
  Rol: string;
}

export interface ApiResponse<T> {
  Success: boolean;
  Message: string;
  Data?: T;
  Errors?: string[];
}

export interface User {
  id: string;
  idUsuario?: number;
  email: string;
  nombre?: string;
  apellidos?: string;
  dni?: string;
  rol?: string;
  cif?: string;
  direccionEmpresa?: string;
  telefonoContacto?: string;
  telefono?: string;
  direccion?: string;
  idEmpresa?: number;
  validado?: boolean;
  documentoIAE?: string;
  password?: string;
  fotoPerfilBase64?: string;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  token: string | null;
}

export interface JwtPayload {
  exp: number;
  iat?: number;
  iss?: string;
  aud?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'?: string;
  'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'?: string;
  'http://schemas.microsoft.com/ws/2008/06/identity/claims/role'?: string;
  [key: string]: any;
}
