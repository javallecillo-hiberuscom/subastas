using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Hosting;
using Subastas.Application.Dtos;
using Subastas.Application.Interfaces.Services;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Infrastructure.Data;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador para funcionalidades específicas del administrador.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly IPdfService _pdfService;
    private readonly SubastaContext _context;
    private readonly ILogger<AdminController> _logger;
    private readonly ISubastaRepository _repo;
    private readonly IWebHostEnvironment _env;

    public AdminController(SubastaContext context, ILogger<AdminController> logger, IPdfService pdfService, ISubastaRepository repo, IWebHostEnvironment env)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pdfService = pdfService ?? throw new ArgumentNullException(nameof(pdfService));
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _env = env ?? throw new ArgumentNullException(nameof(env));
    }

    /// <summary>
    /// Obtiene estadísticas generales del sistema para el dashboard del administrador.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardAdmin()
    {
        try
        {
            _logger.LogInformation("=== INICIO GetDashboardAdmin ===");
            var ahora = DateTime.Now;

            // Estadísticas generales
            _logger.LogInformation("Obteniendo estadísticas generales...");
            var totalUsuarios = await _context.Usuarios.CountAsync();
            _logger.LogInformation($"Total usuarios: {totalUsuarios}");
            var totalEmpresas = await _context.Empresas.CountAsync();
            _logger.LogInformation($"Total empresas: {totalEmpresas}");
            
            var totalVehiculos = await _context.Vehiculos.CountAsync();
            _logger.LogInformation($"Total vehículos: {totalVehiculos}");
            
            var totalSubastas = await _context.Subastas.CountAsync();
            _logger.LogInformation($"Total subastas: {totalSubastas}");

            // Subastas activas con información de quién va ganando
            // Subastas activas - Paso1: Cargar datos con Include
            _logger.LogInformation("Obteniendo subastas activas (paso1: carga de datos)...");
            var subastasActivasData = await _context.Subastas
                .Include(s => s.Vehiculo)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Usuario)
                .Where(s => s.Estado == "activa" && s.FechaFin > ahora)
                .OrderBy(s => s.FechaFin)
                .ToListAsync();
            _logger.LogInformation($"Subastas activas cargadas: {subastasActivasData.Count}");

            // Subastas activas - Paso2: Proyectar en memoria
            _logger.LogInformation("Proyectando subastas activas (paso2: mapeo en memoria)...");
            var subastasActivas = subastasActivasData.Select(s =>
            {
                var tiempoRestante = s.FechaFin - ahora;
                return new
                {
                    s.IdSubasta,
                    s.FechaInicio,
                    s.FechaFin,
                    s.PrecioInicial,
                    s.PrecioActual,
                    tiempoRestante = tiempoRestante > TimeSpan.Zero
                        ? $"{tiempoRestante.Days}d {tiempoRestante.Hours}h {tiempoRestante.Minutes}m"
                        : "Finalizada",
                    vehiculo = new
                    {
                        s.Vehiculo.Marca,
                        s.Vehiculo.Modelo,
                        s.Vehiculo.Anio,
                        s.Vehiculo.Matricula
                    },
                    pujaGanadora = s.Pujas.Any()
                        ? new
                        {
                            cantidad = s.Pujas.OrderByDescending(p => p.Cantidad).First().Cantidad,
                            usuario = new
                            {
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.IdUsuario,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Nombre,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Apellidos,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Email,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Telefono
                            },
                            fechaPuja = s.Pujas.OrderByDescending(p => p.Cantidad).First().FechaPuja
                        }
                        : null,
                    totalPujas = s.Pujas.Count
                };
            }).ToList();
            _logger.LogInformation($"Subastas activas proyectadas: {subastasActivas.Count}");

            // Subastas terminadas with ganador info
            _logger.LogInformation("Obteniendo subastas terminadas (paso1: carga de datos)...");
            var subastasTerminadas = await _context.Subastas
                .Include(s => s.Vehiculo)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Usuario)
                        .ThenInclude(u => u.Empresa)
                .Where(s => s.Estado == "finalizada" || s.FechaFin <= ahora)
                .OrderByDescending(s => s.FechaFin)
                .Take(20) // Últimas20 subastas terminadas
                .ToListAsync();
            _logger.LogInformation($"Subastas terminadas cargadas: {subastasTerminadas.Count}");

            _logger.LogInformation("Proyectando subastas terminadas (paso2: mapeo en memoria)...");
            var subastasTerminadasDto = subastasTerminadas.Select(s => new
                {
                    s.IdSubasta,
                    s.FechaInicio,
                    s.FechaFin,
                    s.PrecioInicial,
                    s.PrecioActual,
                    tiempoFinalizada = ahora > s.FechaFin
                        ? $"Hace {(ahora - s.FechaFin).Days}d {(ahora - s.FechaFin).Hours}h"
                        : "Recién finalizada",
                    vehiculo = new
                    {
                        s.Vehiculo.Marca,
                        s.Vehiculo.Modelo,
                        s.Vehiculo.Anio,
                        s.Vehiculo.Matricula
                    },
                    ganador = s.Pujas.OrderByDescending(p => p.Cantidad).FirstOrDefault() != null
                        ? new
                        {
                            pujaGanadora = s.Pujas.OrderByDescending(p => p.Cantidad).First().Cantidad,
                            usuario = new
                            {
                                IdUsuario = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.IdUsuario,
                                Nombre = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Nombre,
                                Apellidos = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Apellidos,
                                Email = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Email,
                                Telefono = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Telefono,
                                empresa = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Empresa != null
                                    ? new
                                    {
                                        Nombre = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Empresa!.Nombre,
                                        Cif = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Empresa!.Cif
                                    }
                                    : null
                            },
                            fechaPuja = s.Pujas.OrderByDescending(p => p.Cantidad).First().FechaPuja
                        }
                        : null,
                    totalPujas = s.Pujas.Count,
                    sinPujas = s.Pujas.Count ==0
                }).ToList();
            _logger.LogInformation($"Subastas terminadas proyectadas: {subastasTerminadasDto.Count}");

            // Todas las subastas con pujas para la gráfica (ordenadas por cantidad de pujas)
            _logger.LogInformation("Obteniendo TODAS las subastas para gráficas...");
            var todasSubastasConPujas = await _context.Subastas
                .Include(s => s.Vehiculo)
                .Include(s => s.Pujas)
                .OrderByDescending(s => s.Pujas.Count)
                .ToListAsync();
            
            var todasSubastasDto = todasSubastasConPujas.Select(s => new
            {
                s.IdSubasta,
                vehiculo = new
                {
                    s.Vehiculo.Marca,
                    s.Vehiculo.Modelo,
                    s.Vehiculo.Anio
                },
                totalPujas = s.Pujas.Count,
                s.Estado
            }).ToList();
            _logger.LogInformation($"Todas las subastas cargadas: {todasSubastasDto.Count}");

            // Usuarios por estado de validación
            _logger.LogInformation("Obteniendo usuarios por validación...");
            var usuariosPendientes = await _context.Usuarios
                .Where(u => u.Validado ==0 && u.Activo ==1)
                .CountAsync();
            
            var usuariosValidados = await _context.Usuarios
                .Where(u => u.Validado ==1 && u.Activo ==1)
                .CountAsync();
            _logger.LogInformation($"Usuarios pendientes: {usuariosPendientes}, Usuarios validados: {usuariosValidados}");

            _logger.LogInformation("Construyendo resultado final...");
            var resultado = new
            {
                estadisticasGenerales = new
                {
                    totalUsuarios,
                    usuariosPendientes,
                    usuariosValidados,
                    totalEmpresas,
                    totalVehiculos,
                    totalSubastas,
                    subastasActivas = subastasActivas.Count,
                    subastasTerminadas = subastasTerminadasDto.Count
                },
                subastasActivas,
                subastasTerminadas = subastasTerminadasDto,
                todasSubastas = todasSubastasDto
            };

            _logger.LogInformation("=== FIN GetDashboardAdmin (ÉXITO) ===");
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERROR en GetDashboardAdmin: {Message}", ex.Message);
            _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
                _logger.LogError("InnerStackTrace: {InnerStackTrace}", ex.InnerException.StackTrace);
            }
            return StatusCode(500, new 
            { 
                Success = false, 
                Message = "Error al cargar dashboard del administrador", 
                Error = ex.Message,
                StackTrace = ex.StackTrace,
                InnerError = ex.InnerException?.Message 
            });
        }
    }

    /// <summary>
    /// Obtiene lista de usuarios pendientes de validación con sus documentos IAE.
    /// </summary>
    [HttpGet("usuarios-pendientes")]
    public async Task<ActionResult<object>> GetUsuariosPendientes()
    {
        try
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.Empresa)
                .Where(u => u.Validado == 0 && u.Activo == 1)
                .OrderByDescending(u => u.IdUsuario)
                .Select(u => new
                {
                    u.IdUsuario,
                    u.Nombre,
                    u.Apellidos,
                    u.Email,
                    u.Telefono,
                    u.Direccion,
                    u.DocumentoIAE,
                    tieneDocumento = !string.IsNullOrEmpty(u.DocumentoIAE),
                    empresa = u.Empresa != null ? new
                    {
                        Nombre = u.Empresa.Nombre,
                        u.Empresa.Cif
                    } : null
                })
                .ToListAsync();

            return Ok(usuarios);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                Success = false, 
                Message = "Error al cargar usuarios pendientes", 
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Exporta un informe PDF de subastas filtradas por rango de fechas.
    /// Si no se pasan fechas, por defecto: desde hace1 mes hasta hoy.
    /// </summary>
    [HttpGet("exportar-pdf-subastas")]
    public async Task<IActionResult> ExportarPdfSubastas(DateTime? fechaInicio = null, DateTime? fechaFin = null)
    {
        try
        {
            var fin = fechaFin?.Date ?? DateTime.Today;
            var inicio = fechaInicio?.Date ?? fin.AddMonths(-1);

            _logger.LogInformation("ExportarPdfSubastas: from={From} to={To}", inicio, fin);

            var items = await _repo.GetBetweenDatesAsync(inicio, fin);

            var dtos = new List<SubastaDto>();

            foreach (var x in items)
            {
                var dto = new SubastaDto
                {
                    Titulo = !string.IsNullOrWhiteSpace(x.Vehiculo?.Descripcion) ? x.Vehiculo!.Marca + " " + x.Vehiculo.Modelo : string.Empty,
                    Fecha = x.FechaInicio,
                    Precio = x.PrecioActual ?? x.PrecioInicial,
                    Descripcion = x.Vehiculo?.Descripcion
                };

                // Try to include first image as base64 if present
                try
                {
                    var imagenRuta = x.Vehiculo?.ImagenesVehiculo?.FirstOrDefault()?.Ruta;
                    if (!string.IsNullOrWhiteSpace(imagenRuta))
                    {
                        // Look under Uploads and wwwroot/img
                        var candidates = new[]
                        {
                            Path.Combine(_env.ContentRootPath, "Uploads", imagenRuta),
                            Path.Combine(_env.ContentRootPath, "wwwroot", "img", imagenRuta),
                            Path.Combine(_env.ContentRootPath, imagenRuta)
                        };

                        string? foundPath = candidates.FirstOrDefault(p => System.IO.File.Exists(p));
                        if (!string.IsNullOrEmpty(foundPath))
                        {
                            var bytes = await System.IO.File.ReadAllBytesAsync(foundPath);
                            dto.ImagenBase64 = Convert.ToBase64String(bytes);
                        }
                    }
                }
                catch (Exception imgEx)
                {
                    _logger.LogWarning(imgEx, "No se pudo cargar imagen para subasta Id={Id}: {Message}", x.IdSubasta, imgEx.Message);
                }

                dtos.Add(dto);
            }

            var pdf = _pdfService.GenerateSubastasPdf(dtos, inicio, fin);
            var fileName = $"subastas_{inicio:yyyyMMdd}_{fin:yyyyMMdd}.pdf";
            _logger.LogInformation("ExportarPdfSubastas: PDF generado exitosamente, items={Count}", dtos.Count);
            return File(pdf, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF: {Message}", ex.Message);
            return Problem(detail: ex.Message, statusCode:500);
        }
    }

}
