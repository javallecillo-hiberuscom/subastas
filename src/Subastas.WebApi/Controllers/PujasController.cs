using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para operaciones sobre la entidad Puja.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PujasController : ControllerBase
{
    private readonly SubastaContext _context;

    public PujasController(SubastaContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene todas las pujas del sistema.
    /// </summary>
    /// <returns>Lista de pujas con información de vehículo.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPujas()
    {
        try
        {
            var pujas = await _context.Pujas
                .Include(p => p.Subasta)
                    .ThenInclude(s => s.Vehiculo)
                        .ThenInclude(v => v.ImagenesVehiculo)
                .Include(p => p.Usuario)
                .Select(p => new
                {
                    p.IdPuja,
                    p.IdSubasta,
                    p.IdUsuario,
                    p.Cantidad,
                    p.FechaPuja,
                    usuario = new
                    {
                        p.Usuario.IdUsuario,
                        p.Usuario.Nombre,
                        p.Usuario.Apellidos,
                        p.Usuario.Email
                    },
                    subasta = new
                    {
                        p.Subasta.IdSubasta,
                        p.Subasta.FechaInicio,
                        p.Subasta.FechaFin,
                        p.Subasta.Estado,
                        vehiculo = new
                        {
                            p.Subasta.Vehiculo.IdVehiculo,
                            p.Subasta.Vehiculo.Marca,
                            p.Subasta.Vehiculo.Modelo,
                            p.Subasta.Vehiculo.Anio
                        }
                    }
                })
                .OrderByDescending(p => p.FechaPuja)
                .ToListAsync();

            return Ok(pujas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Error al cargar pujas", Error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las pujas activas incluyendo los datos completos del vehículo y sus imágenes.
    /// </summary>
    /// <returns>Lista de pujas activas con información de vehículo.</returns>
    [HttpGet("activas")]
    public async Task<ActionResult<IEnumerable<object>>> GetPujasActivas()
    {
        var ahora = DateTime.Now;

        var pujas = await _context.Pujas
            .Include(p => p.Subasta)
                .ThenInclude(s => s.Vehiculo)
                    .ThenInclude(v => v.ImagenesVehiculo)
            .Where(p => p.Subasta.FechaFin > ahora)
            .Select(p => new
            {
                puja = new
                {
                    p.IdPuja,
                    p.IdSubasta,
                    p.IdUsuario,
                    p.Cantidad,
                    p.FechaPuja
                },
                vehiculo = new
                {
                    p.Subasta.Vehiculo.IdVehiculo,
                    p.Subasta.Vehiculo.Marca,
                    p.Subasta.Vehiculo.Modelo,
                    p.Subasta.Vehiculo.Anio,
                    p.Subasta.Vehiculo.Kilometraje,
                    p.Subasta.Vehiculo.Potencia,
                    imagenes = p.Subasta.Vehiculo.ImagenesVehiculo.Select(img => new
                    {
                        img.IdImagen,
                        img.Nombre,
                        img.Ruta,
                        img.Activo
                    }).ToList()
                }
            })
            .ToListAsync();

        return Ok(pujas);
    }

    /// <summary>
    /// Obtiene las pujas realizadas por un usuario específico.
    /// </summary>
    /// <param name="idUsuario">ID del usuario cuyas pujas se desean obtener.</param>
    /// <returns>Lista de pujas del usuario con información de la subasta y el vehículo.</returns>
    [HttpGet("usuario/{idUsuario}")]
    public async Task<ActionResult<IEnumerable<object>>> GetPujasPorUsuario(int idUsuario)
    {
        try
        {
            var pujas = await _context.Pujas
                .Include(p => p.Subasta)
                    .ThenInclude(s => s.Vehiculo)
                        .ThenInclude(v => v.ImagenesVehiculo)
                .Where(p => p.IdUsuario == idUsuario)
            .Select(p => new
            {
                p.IdPuja,
                p.Cantidad,
                p.FechaPuja,
                p.IdSubasta,
                p.IdUsuario,
                subasta = new
                {
                    p.Subasta.IdSubasta,
                    p.Subasta.IdVehiculo,
                    p.Subasta.FechaInicio,
                    p.Subasta.FechaFin,
                    p.Subasta.PrecioInicial,
                    p.Subasta.PrecioActual,
                    p.Subasta.IncrementoMinimo,
                    p.Subasta.Estado,
                    vehiculo = new
                    {
                        p.Subasta.Vehiculo.IdVehiculo,
                        p.Subasta.Vehiculo.Marca,
                        p.Subasta.Vehiculo.Modelo,
                        p.Subasta.Vehiculo.Anio,
                        p.Subasta.Vehiculo.Kilometraje,
                        p.Subasta.Vehiculo.Potencia,
                        p.Subasta.Vehiculo.Color,
                        p.Subasta.Vehiculo.TipoMotor,
                        p.Subasta.Vehiculo.TipoCarroceria,
                        p.Subasta.Vehiculo.Descripcion,
                        p.Subasta.Vehiculo.Matricula,
                        p.Subasta.Vehiculo.NumeroPuertas,
                        imagenes = p.Subasta.Vehiculo.ImagenesVehiculo.Select(img => new
                        {
                            img.IdImagen,
                            img.Nombre,
                            img.Ruta,
                            img.Activo
                        }).ToList()
                    }
                }
            })
            .OrderByDescending(p => p.FechaPuja)
            .ToListAsync();

            return Ok(pujas);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Error al cargar pujas del usuario", Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Realiza una nueva puja.
    /// </summary>
    /// <param name="request">Datos de la puja.</param>
    /// <returns>NoContent si la puja se crea correctamente.</returns>
    [HttpPost]
    public async Task<IActionResult> PostPuja([FromBody] PujaRequest request)
    {
        var subasta = await _context.Subastas.FindAsync(request.IdSubasta);
        var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);

        if (subasta == null || usuario == null)
            return BadRequest("Subasta o usuario no válido.");

        // Validar que el usuario NO sea administrador
        if (usuario.Rol?.Trim().ToLower() == "admin")
            return BadRequest(new 
            { 
                mensaje = "Los administradores no pueden realizar pujas. Los vehículos son propiedad del sistema.",
                esAdmin = true
            });

        // Validar que el usuario esté validado (COMENTADO PARA DESARROLLO)
        // if (usuario.Validado == 0)
        //     return BadRequest(new 
        //     { 
        //         mensaje = "Tu cuenta debe estar validada para poder pujar. Por favor, sube tu documento IAE y espera la validación del administrador.",
        //         requiereValidacion = true
        //     });

        // Validar que el usuario tenga documento IAE subido
        if (string.IsNullOrEmpty(usuario.DocumentoIAE))
            return BadRequest(new 
            { 
                mensaje = "Debes subir tu documento IAE antes de poder pujar.",
                requiereDocumento = true
            });

        var puja = new Puja
        {
            IdSubasta = request.IdSubasta,
            IdUsuario = request.IdUsuario,
            Cantidad = request.Cantidad,
            FechaPuja = request.FechaPuja
        };

        _context.Pujas.Add(puja);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

/// <summary>
/// Modelo de datos para crear una puja.
/// </summary>
public class PujaRequest
{
    public int IdSubasta { get; set; }
    public int IdUsuario { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime FechaPuja { get; set; }
}
