import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, BehaviorSubject, tap, catchError, throwError, map } from 'rxjs';
import { LoginRequest, LoginResponse, User, AuthState, JwtPayload, ApiResponse } from '../models/auth.models';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private getApiUrl(): string {
    const baseUrl = window.location.hostname === 'localhost' 
      ? 'http://localhost:56801' 
      : 'https://subastaswebapi20260202162157-f3frc5dfgdata6cx.canadacentral-01.azurewebsites.net';
    return `${baseUrl}/api/usuarios`;
  }
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'auth_user';

  private http = inject(HttpClient);
  private router = inject(Router);

  private authState = signal<AuthState>({
    isAuthenticated: false,
    user: null,
    token: null
  });

  // Señales computadas para acceso reactivo
  readonly isAuthenticated = computed(() => this.authState().isAuthenticated);
  readonly currentUser = computed(() => this.authState().user);
  readonly token = computed(() => this.authState().token);

  constructor() {
    this.loadAuthState();
  }

  /**
   * Carga el estado de autenticación desde localStorage
   */
  private loadAuthState(): void {
    const token = localStorage.getItem(this.TOKEN_KEY);
    const userJson = localStorage.getItem(this.USER_KEY);

    if (token && userJson) {
      try {
        let user = JSON.parse(userJson) as User;
        
        // Migración: Si no tiene idUsuario pero sí tiene id, convertirlo
        if (!user.idUsuario && user.id) {
          user.idUsuario = parseInt(user.id) || undefined;
          // Guardar el usuario actualizado
          localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        }
        
        this.authState.set({
          isAuthenticated: true,
          user,
          token
        });
      } catch (error) {
        this.clearAuthState();
      }
    }
  }

  /**
   * Realiza el login del usuario
   */
  login(email: string, password: string): Observable<LoginResponse> {
    const credentials: LoginRequest = {
      Email: email,
      Password: password
    };
    
    return this.http.post<ApiResponse<LoginResponse>>(`${this.getApiUrl()}/login`, credentials).pipe(
      map(apiResponse => {
        console.log('Respuesta del backend:', apiResponse);
        // Compatibilidad con camelCase y PascalCase
        const success = (apiResponse as any).success ?? apiResponse.Success;
        const data = (apiResponse as any).data ?? apiResponse.Data;
        const message = (apiResponse as any).message ?? apiResponse.Message;
        
        console.log('success:', success);
        console.log('data:', data);
        
        if (!success || !data) {
          throw new Error(message || 'Error en el login');
        }
        return data;
      }),
      tap(response => {
        console.log('Procesando respuesta de login:', response);
        // Crear el usuario desde la respuesta del backend (ahora en camelCase)
        const user: User = {
          id: response.idUsuario?.toString() || response.IdUsuario?.toString() || '',
          idUsuario: response.idUsuario || response.IdUsuario,
          email: response.email || response.Email,
          nombre: response.nombreCompleto || response.NombreCompleto,
          rol: response.rol || response.Rol,
          validado: response.validado !== undefined ? response.validado : (response.Validado !== undefined ? response.Validado : true)
        };
        console.log('Usuario creado:', user);
        this.setAuthState(response.token || response.Token, user);
      }),
      catchError(error => {
        console.error('Error en login:', error);
        return throwError(() => error);
      })
    );
  }

  /**
   * Cierra la sesión del usuario
   */
  logout(): void {
    this.clearAuthState();
    this.router.navigate(['/login']);
  }

  /**
   * Actualiza el usuario actual
   */
  setCurrentUser(user: User): void {
    const currentState = this.authState();
    this.authState.set({
      ...currentState,
      user
    });
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
  }

  /**
   * Establece el estado de autenticación
   */
  private setAuthState(token: string, user: User): void {
    localStorage.setItem(this.TOKEN_KEY, token);
    localStorage.setItem(this.USER_KEY, JSON.stringify(user));
    
    this.authState.set({
      isAuthenticated: true,
      user,
      token
    });
  }

  /**
   * Limpia el estado de autenticación
   */
  private clearAuthState(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    
    this.authState.set({
      isAuthenticated: false,
      user: null,
      token: null
    });
  }

  /**
   * Obtiene el token actual
   */
  getToken(): string | null {
    return this.authState().token;
  }

  /**
   * Verifica si el usuario está autenticado y el token es válido
   */
  checkAuthentication(): boolean {
    // Siempre intentar recargar desde localStorage por si se perdió el estado
    const token = localStorage.getItem(this.TOKEN_KEY);
    const userJson = localStorage.getItem(this.USER_KEY);
    
    if (!token || !userJson) {
      this.clearAuthState();
      return false;
    }
    
    // Verificar si el token ha expirado
    if (this.isTokenExpired(token)) {
      console.log('Token expirado, limpiando sesión');
      this.clearAuthState();
      return false;
    }
    
    // SIEMPRE recargar el estado para asegurar consistencia
    try {
      const user = JSON.parse(userJson) as User;
      
      // Migración: Si no tiene idUsuario pero sí tiene id, convertirlo
      if (!user.idUsuario && user.id) {
        user.idUsuario = parseInt(user.id) || undefined;
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
      }
      
      this.authState.set({
        isAuthenticated: true,
        user,
        token
      });
      
      return true;
    } catch (error) {
      console.error('Error parseando usuario:', error);
      this.clearAuthState();
      return false;
    }
  }
  
  /**
   * Verifica si el token JWT ha expirado
   */
  private isTokenExpired(token: string): boolean {
    try {
      const payload = this.parseJwt(token);
      const exp = payload.exp;
      
      if (!exp) {
        return false; // Si no tiene exp, asumimos que no expira
      }
      
      const now = Math.floor(Date.now() / 1000);
      return exp < now;
    } catch (error) {
      console.error('Error verificando expiración del token:', error);
      return true; // Si hay error, asumir que está expirado
    }
  }

  /**
   * Decodifica el token JWT y extrae la información del usuario
   */
  private decodeToken(token: string): User {
    try {
      const payload = this.parseJwt(token);
      
      const id = payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier'] || '';
      
      return {
        id: id,
        idUsuario: parseInt(id) || undefined, // Convertir id a número para idUsuario
        email: payload['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] || '',
        rol: payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || ''
      };
    } catch (error) {
      console.error('Error decodificando token:', error);
      throw error;
    }
  }

  /**
   * Parsea el JWT para extraer el payload
   */
  private parseJwt(token: string): JwtPayload {
    try {
      const base64Url = token.split('.')[1];
      const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split('')
          .map(c => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
          .join('')
      );
      return JSON.parse(jsonPayload);
    } catch (error) {
      console.error('Error parseando JWT:', error);
      throw error;
    }
  }
}
