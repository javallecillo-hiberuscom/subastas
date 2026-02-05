import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Vehiculo, Puja, PujaRequest } from '../models/app.models';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {
  private getApiUrl(): string {
    const baseUrl = window.location.hostname === 'localhost' 
      ? 'http://localhost:56801' 
      : 'https://subastas-api-borox.azurewebsites.net';
    return `${baseUrl}/api/vehiculos`;
  }
  private http = inject(HttpClient);

  vehiculosEnSubasta = signal<Vehiculo[]>([]);

  getVehiculos(): Observable<Vehiculo[]> {
    return this.http.get<Vehiculo[]>(this.getApiUrl());
  }

  getVehiculosEnSubasta(): Observable<Vehiculo[]> {
    const baseUrl = window.location.hostname === 'localhost' 
      ? 'http://localhost:56801' 
      : 'https://subastas-api-borox.azurewebsites.net';
    return this.http.get<Vehiculo[]>(`${baseUrl}/api/pujas`);
  }

  getVehiculoById(id: number): Observable<Vehiculo> {
    return this.http.get<Vehiculo>(`${this.getApiUrl()}/${id}`);
  }

  crearVehiculo(vehiculo: any): Observable<Vehiculo> {
    return this.http.post<Vehiculo>(this.getApiUrl(), vehiculo);
  }

  createVehiculo(vehiculo: any): Observable<Vehiculo> {
    return this.crearVehiculo(vehiculo);
  }

  actualizarVehiculo(id: number, vehiculo: any): Observable<void> {
    return this.http.put<void>(`${this.getApiUrl()}/${id}`, vehiculo);
  }

  updateVehiculo(id: number, vehiculo: any): Observable<void> {
    return this.actualizarVehiculo(id, vehiculo);
  }

  eliminarVehiculo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.getApiUrl()}/${id}`);
  }

  deleteVehiculo(id: number): Observable<void> {
    return this.eliminarVehiculo(id);
  }
}

@Injectable({
  providedIn: 'root'
})
export class PujaService {
  private getApiUrl(): string {
    const baseUrl = window.location.hostname === 'localhost' 
      ? 'http://localhost:56801' 
      : 'https://subastas-api-borox.azurewebsites.net';
    return `${baseUrl}/api/pujas`;
  }
  private http = inject(HttpClient);

  misPujas = signal<Puja[]>([]);

  getPujas(): Observable<Puja[]> {
    return this.http.get<Puja[]>(this.getApiUrl());
  }

  getPujasActivas(): Observable<Puja[]> {
    return this.http.get<Puja[]>(this.getApiUrl());
  }

  getPujaById(id: number): Observable<Puja> {
    return this.http.get<Puja>(`${this.getApiUrl()}/${id}`);
  }

  getMisPujas(): Observable<Puja[]> {
    return this.http.get<Puja[]>(`${this.getApiUrl()}/mis-pujas`);
  }

  realizarPuja(request: PujaRequest): Observable<void> {
    return this.http.post<void>(`${this.getApiUrl()}/pujar`, request);
  }

  crearPuja(puja: Partial<Puja>): Observable<Puja> {
    return this.http.post<Puja>(this.getApiUrl(), puja);
  }

  createPuja(puja: Partial<Puja>): Observable<Puja> {
    return this.crearPuja(puja);
  }

  actualizarPuja(id: number, puja: Partial<Puja>): Observable<void> {
    return this.http.put<void>(`${this.getApiUrl()}/${id}`, puja);
  }

  updatePuja(id: number, puja: Partial<Puja>): Observable<void> {
    return this.actualizarPuja(id, puja);
  }

  eliminarPuja(id: number): Observable<void> {
    return this.http.delete<void>(`${this.getApiUrl()}/${id}`);
  }

  deletePuja(id: number): Observable<void> {
    return this.eliminarPuja(id);
  }
}
