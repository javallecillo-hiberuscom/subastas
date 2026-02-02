import { Injectable, signal, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Vehiculo, Puja, PujaRequest } from '../models/app.models';

@Injectable({
  providedIn: 'root'
})
export class VehiculoService {
  private readonly API_URL = '/api/vehiculos';
  private http = inject(HttpClient);

  vehiculosEnSubasta = signal<Vehiculo[]>([]);

  getVehiculos(): Observable<Vehiculo[]> {
    return this.http.get<Vehiculo[]>(this.API_URL);
  }

  getVehiculosEnSubasta(): Observable<Vehiculo[]> {
    // Usar el endpoint base de pujas
    return this.http.get<Vehiculo[]>('/api/pujas');
  }

  getVehiculoById(id: number): Observable<Vehiculo> {
    return this.http.get<Vehiculo>(`${this.API_URL}/${id}`);
  }

  crearVehiculo(vehiculo: any): Observable<Vehiculo> {
    return this.http.post<Vehiculo>(this.API_URL, vehiculo);
  }

  createVehiculo(vehiculo: any): Observable<Vehiculo> {
    return this.crearVehiculo(vehiculo);
  }

  actualizarVehiculo(id: number, vehiculo: any): Observable<void> {
    return this.http.put<void>(`${this.API_URL}/${id}`, vehiculo);
  }

  updateVehiculo(id: number, vehiculo: any): Observable<void> {
    return this.actualizarVehiculo(id, vehiculo);
  }

  eliminarVehiculo(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }

  deleteVehiculo(id: number): Observable<void> {
    return this.eliminarVehiculo(id);
  }
}

@Injectable({
  providedIn: 'root'
})
export class PujaService {
  private readonly API_URL = '/api/pujas';
  private http = inject(HttpClient);

  misPujas = signal<Puja[]>([]);

  getPujas(): Observable<Puja[]> {
    return this.http.get<Puja[]>(this.API_URL);
  }

  getPujasActivas(): Observable<Puja[]> {
    // Usar el endpoint base y filtrar en el frontend si es necesario
    return this.http.get<Puja[]>(this.API_URL);
  }

  getPujaById(id: number): Observable<Puja> {
    return this.http.get<Puja>(`${this.API_URL}/${id}`);
  }

  getMisPujas(): Observable<Puja[]> {
    return this.http.get<Puja[]>(`${this.API_URL}/mis-pujas`);
  }

  realizarPuja(request: PujaRequest): Observable<void> {
    return this.http.post<void>(`${this.API_URL}/pujar`, request);
  }

  crearPuja(puja: Partial<Puja>): Observable<Puja> {
    return this.http.post<Puja>(this.API_URL, puja);
  }

  createPuja(puja: Partial<Puja>): Observable<Puja> {
    return this.crearPuja(puja);
  }

  actualizarPuja(id: number, puja: Partial<Puja>): Observable<void> {
    return this.http.put<void>(`${this.API_URL}/${id}`, puja);
  }

  updatePuja(id: number, puja: Partial<Puja>): Observable<void> {
    return this.actualizarPuja(id, puja);
  }

  eliminarPuja(id: number): Observable<void> {
    return this.http.delete<void>(`${this.API_URL}/${id}`);
  }

  deletePuja(id: number): Observable<void> {
    return this.eliminarPuja(id);
  }
}
