import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UsuarioDetalle } from '../models/app.models';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  private readonly API_URL = '/api/usuarios';
  private http = inject(HttpClient);

  getUsuarios(): Observable<UsuarioDetalle[]> {
    return this.http.get<UsuarioDetalle[]>(this.API_URL);
  }

  getUsuarioById(id: number): Observable<UsuarioDetalle> {
    return this.http.get<UsuarioDetalle>(`${this.API_URL}/${id}`);
  }

  getMiPerfil(): Observable<UsuarioDetalle> {
    return this.http.get<UsuarioDetalle>(`${this.API_URL}/perfil`);
  }

  actualizarPerfil(usuario: Partial<UsuarioDetalle>): Observable<void> {
    return this.http.put<void>(`${this.API_URL}/perfil`, usuario);
  }

  validarUsuario(id: number, validado: boolean): Observable<void> {
    return this.http.put<void>(`${this.API_URL}/${id}/validar`, { validado });
  }

  eliminarUsuario(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }

  subirDocumentoIAE(archivo: File): Observable<void> {
    const formData = new FormData();
    formData.append('documento', archivo);
    return this.http.post<void>(`${this.API_URL}/iae`, formData);
  }
}
