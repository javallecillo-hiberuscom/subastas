import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { UsuarioDetalle } from '../models/app.models';

@Injectable({
  providedIn: 'root'
})
export class UsuarioService {
  private getApiUrl(): string {
    const baseUrl = window.location.hostname === 'localhost' 
      ? 'http://localhost:56801' 
      : 'https://subastas-api-borox.azurewebsites.net';
    return `${baseUrl}/api/usuarios`;
  }
  private http = inject(HttpClient);

  getUsuarios(): Observable<UsuarioDetalle[]> {
    return this.http.get<UsuarioDetalle[]>(this.getApiUrl());
  }

  getUsuarioById(id: number): Observable<UsuarioDetalle> {
    return this.http.get<UsuarioDetalle>(`${this.getApiUrl()}/${id}`);
  }

  getMiPerfil(): Observable<UsuarioDetalle> {
    return this.http.get<UsuarioDetalle>(`${this.getApiUrl()}/perfil`);
  }

  actualizarPerfil(usuario: Partial<UsuarioDetalle>): Observable<void> {
    return this.http.put<void>(`${this.getApiUrl()}/perfil`, usuario);
  }

  validarUsuario(id: number, validado: boolean): Observable<void> {
    return this.http.put<void>(`${this.getApiUrl()}/${id}/validar`, { validado });
  }

  eliminarUsuario(id: number): Observable<void> {
    return this.http.delete<void>(`${this.getApiUrl()}/${id}`);
  }

  subirDocumentoIAE(archivo: File): Observable<void> {
    const formData = new FormData();
    formData.append('documento', archivo);
    return this.http.post<void>(`${this.getApiUrl()}/iae`, formData);
  }
}
