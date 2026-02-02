import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService, DashboardAdmin } from '../services/admin.service';

@Component({
  selector: 'app-dashboard-admin',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-admin.component.html',
  styleUrls: ['./dashboard-admin.component.css']
})
export class DashboardAdminComponent implements OnInit {
  dashboard = signal<DashboardAdmin | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.cargarDashboard();
  }

  cargarDashboard() {
    this.isLoading.set(true);
    this.error.set(null);

    this.adminService.getDashboard().subscribe({
      next: (data: DashboardAdmin) => {
        this.dashboard.set(data);
        this.isLoading.set(false);
      },
      error: (err: any) => {
        console.error('Error cargando dashboard admin:', err);
        this.error.set('Error al cargar el dashboard del administrador');
        this.isLoading.set(false);
      }
    });
  }
}
