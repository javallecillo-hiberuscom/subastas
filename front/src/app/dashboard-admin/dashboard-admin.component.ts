import { Component, OnInit, signal, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { AdminService, DashboardAdmin } from '../services/admin.service';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-admin',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './dashboard-admin.component.html',
  styleUrls: ['./dashboard-admin.component.css']
})
export class DashboardAdminComponent implements OnInit, AfterViewInit {
  @ViewChild('subastasChart') subastasChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('pujasChart') pujasChartRef?: ElementRef<HTMLCanvasElement>;
  
  dashboard = signal<DashboardAdmin | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);
  
  private subastasChart?: Chart;
  private pujasChart?: Chart;

  constructor(private adminService: AdminService) {}

  ngOnInit() {
    this.cargarDashboard();
  }

  ngAfterViewInit() {
    // Los gráficos se crearán después de cargar los datos
  }

  cargarDashboard() {
    this.isLoading.set(true);
    this.error.set(null);

    this.adminService.getDashboard().subscribe({
      next: (data: DashboardAdmin) => {
        this.dashboard.set(data);
        this.isLoading.set(false);
        // Crear gráficos después de cargar datos
        setTimeout(() => this.crearGraficos(), 100);
      },
      error: (err: any) => {
        console.error('Error cargando dashboard admin:', err);
        this.error.set('Error al cargar el dashboard del administrador');
        this.isLoading.set(false);
      }
    });
  }

  private crearGraficos() {
    const data = this.dashboard();
    if (!data) return;

    this.crearGraficoSubastas(data);
    this.crearGraficoPujas(data);
  }

  private crearGraficoSubastas(data: DashboardAdmin) {
    if (!this.subastasChartRef) return;

    const ctx = this.subastasChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    // Destruir gráfico anterior si existe
    if (this.subastasChart) {
      this.subastasChart.destroy();
    }

    const config: ChartConfiguration = {
      type: 'doughnut',
      data: {
        labels: ['Activas', 'Terminadas'],
        datasets: [{
          data: [
            data.estadisticasGenerales.subastasActivas,
            data.estadisticasGenerales.subastasTerminadas
          ],
          backgroundColor: ['#4CAF50', '#2196F3'],
          borderWidth: 2,
          borderColor: '#fff'
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'bottom'
          },
          title: {
            display: true,
            text: 'Estado de Subastas',
            font: { size: 16, weight: 'bold' }
          }
        }
      }
    };

    this.subastasChart = new Chart(ctx, config);
  }

  private crearGraficoPujas(data: DashboardAdmin) {
    if (!this.pujasChartRef) return;

    const ctx = this.pujasChartRef.nativeElement.getContext('2d');
    if (!ctx) return;

    // Destruir gráfico anterior si existe
    if (this.pujasChart) {
      this.pujasChart.destroy();
    }

    // Preparar datos de pujas por subasta (top 5)
    const subastasConPujas = data.subastasActivas
      .map(s => ({
        nombre: `${s.vehiculo.marca} ${s.vehiculo.modelo}`,
        pujas: s.totalPujas
      }))
      .sort((a, b) => b.pujas - a.pujas)
      .slice(0, 5);

    const config: ChartConfiguration = {
      type: 'bar',
      data: {
        labels: subastasConPujas.map(s => s.nombre),
        datasets: [{
          label: 'Número de Pujas',
          data: subastasConPujas.map(s => s.pujas),
          backgroundColor: '#FF9800',
          borderColor: '#F57C00',
          borderWidth: 2
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            display: false
          },
          title: {
            display: true,
            text: 'Pujas por Subasta (Top 5)',
            font: { size: 16, weight: 'bold' }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            ticks: {
              stepSize: 1
            }
          }
        }
      }
    };

    this.pujasChart = new Chart(ctx, config);
  }
}
