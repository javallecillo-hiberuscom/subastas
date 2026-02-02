using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador para funcionalidades específicas del administrador.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class AdminController : ControllerBase
{
    private readonly SubastaContext _context;

    public AdminController(SubastaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene estadísticas generales del sistema para el dashboard del administrador.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<object>> GetDashboardAdmin()
    {
        try
        {
            var ahora = DateTime.Now;

            // Estadísticas generales
            var totalUsuarios = await _context.Usuarios.CountAsync();
            var totalEmpresas = await _context.Empresas.CountAsync();
            var totalVehiculos = await _context.Vehiculos.CountAsync();
            var totalSubastas = await _context.Subastas.CountAsync();

            // Subastas activas con información de quién va ganando
            var subastasActivas = await _context.Subastas
                .Include(s => s.Vehiculo)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Usuario)
                .Where(s => s.Estado == "activa" && s.FechaFin > ahora)
                .Select(s => new
                {
                    s.IdSubasta,
                    s.FechaInicio,
                    s.FechaFin,
                    s.PrecioInicial,
                    s.PrecioActual,
                    tiempoRestante = s.FechaFin > ahora 
                        ? $"{(s.FechaFin - ahora).Days}d {(s.FechaFin - ahora).Hours}h {(s.FechaFin - ahora).Minutes}m"
                        : "Finalizada",
                    vehiculo = new
                    {
                        s.Vehiculo.Marca,
                        s.Vehiculo.Modelo,
                        s.Vehiculo.Anio,
                        s.Vehiculo.Matricula
                    },
                    pujaGanadora = s.Pujas.OrderByDescending(p => p.Cantidad).FirstOrDefault() != null
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
                })
                .OrderBy(s => s.FechaFin)
                .ToListAsync();

            // Subastas terminadas con información del ganador
            var subastasTerminadas = await _context.Subastas
                .Include(s => s.Vehiculo)
                .Include(s => s.Pujas)
                    .ThenInclude(p => p.Usuario)
                .Where(s => s.Estado == "finalizada" || s.FechaFin <= ahora)
                .Select(s => new
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
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.IdUsuario,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Nombre,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Apellidos,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Email,
                                s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Telefono,
                                empresa = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Empresa != null
                                    ? new
                                    {
                                        Nombre = s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Empresa.Nombre,
                                        s.Pujas.OrderByDescending(p => p.Cantidad).First().Usuario.Empresa.Cif
                                    }
                                    : null
                            },
                            fechaPuja = s.Pujas.OrderByDescending(p => p.Cantidad).First().FechaPuja
                        }
                        : null,
                    totalPujas = s.Pujas.Count,
                    sinPujas = s.Pujas.Count == 0
                })
                .OrderByDescending(s => s.FechaFin)
                .Take(20) // Últimas 20 subastas terminadas
                .ToListAsync();

            // Usuarios por estado de validación
            var usuariosPendientes = await _context.Usuarios
                .Where(u => u.Validado == 0 && u.Activo == 1)
                .CountAsync();
            
            var usuariosValidados = await _context.Usuarios
                .Where(u => u.Validado == 1 && u.Activo == 1)
                .CountAsync();

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
                    subastasTerminadas = subastasTerminadas.Count
                },
                subastasActivas,
                subastasTerminadas
            };

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new 
            { 
                Success = false, 
                Message = "Error al cargar dashboard del administrador", 
                Error = ex.Message,
                StackTrace = ex.StackTrace 
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
}
