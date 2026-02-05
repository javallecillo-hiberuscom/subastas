import { Component, OnInit, signal, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AdminService, DashboardAdmin } from '../services/admin.service';
import { Chart, ChartConfiguration, registerables } from 'chart.js';

Chart.register(...registerables);

@Component({
  selector: 'app-dashboard-admin',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './dashboard-admin.component.html',
  styleUrls: ['./dashboard-admin.component.css']
})
export class DashboardAdminComponent implements OnInit, AfterViewInit {
    exportarExcel(tipo: 'activas' | 'terminadas') {
      const data = this.dashboard();
      let rows: any[] = [];
      let headers: string[] = [];
      if (tipo === 'activas' && data?.subastasActivas) {
        headers = ['ID', 'Marca', 'Modelo', 'Año', 'Matricula', 'Precio Inicial', 'Precio Actual', 'Ganador', 'Email', 'Teléfono', 'Total Pujas', 'Tiempo Restante'];
        rows = data.subastasActivas.map(s => [
          s.idSubasta,
          s.vehiculo.marca,
          s.vehiculo.modelo,
          s.vehiculo.anio,
          s.vehiculo.matricula,
          s.precioInicial,
          s.precioActual,
          s.pujaGanadora ? `${s.pujaGanadora.usuario.nombre} ${s.pujaGanadora.usuario.apellidos}` : '',
          s.pujaGanadora ? s.pujaGanadora.usuario.email : '',
          s.pujaGanadora ? s.pujaGanadora.usuario.telefono : '',
          s.totalPujas,
          s.tiempoRestante
        ]);
      }
      if (tipo === 'terminadas' && data?.subastasTerminadas) {
        headers = ['ID', 'Marca', 'Modelo', 'Año', 'Matricula', 'Precio Inicial', 'Precio Final', 'Ganador', 'Email', 'Teléfono', 'Empresa', 'CIF', 'Total Pujas', 'Finalizada'];
        rows = data.subastasTerminadas.map(s => [
          s.idSubasta,
          s.vehiculo.marca,
          s.vehiculo.modelo,
          s.vehiculo.anio,
          s.vehiculo.matricula,
          s.precioInicial,
          s.precioActual,
          s.ganador ? `${s.ganador.usuario.nombre} ${s.ganador.usuario.apellidos}` : '',
          s.ganador ? s.ganador.usuario.email : '',
          s.ganador ? s.ganador.usuario.telefono : '',
          s.ganador && s.ganador.usuario.empresa ? s.ganador.usuario.empresa.nombre : '',
          s.ganador && s.ganador.usuario.empresa ? s.ganador.usuario.empresa.cif : '',
          s.totalPujas,
          s.tiempoFinalizada
        ]);
      }
      // Generar CSV
      let csv = headers.join(',') + '\n';
      csv += rows.map(r => r.map((x: any) => `"${x ?? ''}"`).join(',')).join('\n');
      const blob = new Blob([csv], { type: 'text/csv' });
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = `subastas-${tipo}-${new Date().toISOString().slice(0,10)}.csv`;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    }
  @ViewChild('subastasChart') subastasChartRef?: ElementRef<HTMLCanvasElement>;
  @ViewChild('pujasChart') pujasChartRef?: ElementRef<HTMLCanvasElement>;
  
  dashboard = signal<DashboardAdmin | null>(null);
  isLoading = signal(false);
  error = signal<string | null>(null);

  fechaInicio: string = '';
  fechaFin: string = '';

  private subastasChart?: Chart;
  private pujasChart?: Chart;

  constructor(private adminService: AdminService) {}
  exportarPDF() {
    if (!this.fechaInicio || !this.fechaFin) {
      alert('Por favor selecciona ambas fechas para exportar el PDF.');
      return;
    }
    this.adminService.exportarPdfSubastas(this.fechaInicio, this.fechaFin).subscribe({
      next: (blob: Blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `informe-subastas-${this.fechaInicio}-${this.fechaFin}.pdf`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        alert('Error al generar el PDF: ' + (err?.error?.Message || 'Error desconocido'));
      }
    });
  }

  ngOnInit() {
    // Por defecto: desde un mes atrás hasta hoy
    const hoy = new Date();
    const mesAtras = new Date();
    mesAtras.setMonth(hoy.getMonth() - 1);
    this.fechaFin = hoy.toISOString().slice(0, 10);
    this.fechaInicio = mesAtras.toISOString().slice(0, 10);
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

    // Preparar datos de pujas por subasta (top 5) - USAR TODAS LAS SUBASTAS
    const subastasConPujas = (data.todasSubastas || [])
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
