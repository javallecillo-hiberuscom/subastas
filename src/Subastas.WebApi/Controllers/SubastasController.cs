using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para operaciones sobre la entidad Subasta.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class SubastasController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly IWebHostEnvironment _env;

    public SubastasController(SubastaContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    /// <summary>
    /// Obtiene todas las subastas con opción de filtrar solo activas o vencidas.
    /// </summary>
    /// <param name="activas">Si es true, devuelve solo subastas activas. Si es false, devuelve solo vencidas.</param>
    /// <returns>Lista de subastas con información de vehículo e imágenes en Base64.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetSubastas([FromQuery] bool activas = false)
    {
        try
        {
            var ahora = DateTime.Now;
            var query = _context.Subastas
                .Include(s => s.Vehiculo)
                    .ThenInclude(v => v.ImagenesVehiculo)
                .AsQueryable();

            if (activas)
            {
                // Solo subastas activas (en curso)
                query = query.Where(s => s.Estado == "activa" && s.FechaFin > ahora);
            }
            else
            {
                // Solo subastas vencidas (finalizadas o fecha pasada)
                query = query.Where(s => s.Estado == "finalizada" || s.FechaFin <= ahora);
            }

            var subastas = await query
                .OrderByDescending(s => s.FechaFin)
                .ToListAsync();

            var resultado = subastas.Select(s => new
            {
                s.IdSubasta,
                s.IdVehiculo,
                s.FechaInicio,
                s.FechaFin,
                s.PrecioInicial,
                s.PrecioActual,
                s.IncrementoMinimo,
                s.Estado,
                vehiculo = new
                {
                    s.Vehiculo.IdVehiculo,
                    s.Vehiculo.Marca,
                    s.Vehiculo.Modelo,
                    s.Vehiculo.Anio,
                    s.Vehiculo.Kilometraje,
                    s.Vehiculo.Potencia,
                    s.Vehiculo.Matricula,
                    s.Vehiculo.Color,
                    s.Vehiculo.NumeroPuertas,
                    s.Vehiculo.Descripcion,
                    s.Vehiculo.TipoCarroceria,
                    s.Vehiculo.TipoMotor,
                    imagenes = s.Vehiculo.ImagenesVehiculo
                        .Select(img => ConvertirImagenABase64(img))
                        .ToList()
                }
            }).ToList();

            return Ok(resultado);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Error al cargar subastas", Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Obtiene una subasta por ID con detalles del vehículo.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetSubasta(int id)
    {
        var subasta = await _context.Subastas
            .Include(s => s.Vehiculo)
                .ThenInclude(v => v.ImagenesVehiculo)
            .FirstOrDefaultAsync(s => s.IdSubasta == id);

        if (subasta == null)
            return NotFound();

        var resultado = new
        {
            subasta.IdSubasta,
            subasta.IdVehiculo,
            subasta.FechaInicio,
            subasta.FechaFin,
            subasta.PrecioInicial,
            subasta.PrecioActual,
            subasta.IncrementoMinimo,
            subasta.Estado,
            vehiculo = new
            {
                subasta.Vehiculo.IdVehiculo,
                subasta.Vehiculo.Marca,
                subasta.Vehiculo.Modelo,
                subasta.Vehiculo.Anio,
                subasta.Vehiculo.Kilometraje,
                subasta.Vehiculo.Potencia,
                subasta.Vehiculo.Matricula,
                subasta.Vehiculo.Color,
                subasta.Vehiculo.NumeroPuertas,
                subasta.Vehiculo.Descripcion,
                subasta.Vehiculo.TipoCarroceria,
                subasta.Vehiculo.TipoMotor,
                imagenes = subasta.Vehiculo.ImagenesVehiculo
                    .Select(img => ConvertirImagenABase64(img))
                    .ToList()
            }
        };

        return Ok(resultado);
    }

    /// <summary>
    /// Convierte una imagen física a Base64.
    /// </summary>
    private object ConvertirImagenABase64(ImagenVehiculo img)
    {
        try
        {
            // Convertir ruta relativa a ruta física
            if (string.IsNullOrEmpty(img.Ruta))
                return new { img.IdImagen, img.Nombre, img.Ruta, img.Activo, imagenBase64 = (string?)null };
                
            var rutaRelativa = img.Ruta.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rutaFisica = Path.Combine(_env.ContentRootPath, rutaRelativa);

            if (System.IO.File.Exists(rutaFisica))
            {
                var bytes = System.IO.File.ReadAllBytes(rutaFisica);
                var base64 = Convert.ToBase64String(bytes);

                return new
                {
                    img.IdImagen,
                    img.Nombre,
                    img.Ruta,
                    img.Activo,
                    imagenBase64 = base64
                };
            }
        }
        catch
        {
            // Si hay error, devolver sin Base64
        }

        return new
        {
            img.IdImagen,
            img.Nombre,
            img.Ruta,
            img.Activo,
            imagenBase64 = (string?)null
        };
    }
}
