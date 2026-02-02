using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para operaciones sobre vehículos.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class VehiculosController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly IWebHostEnvironment _env;

    public VehiculosController(SubastaContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    /// <summary>
    /// Obtiene todos los vehículos con sus imágenes.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetVehiculos()
    {
        var vehiculos = await _context.Vehiculos
            .Include(v => v.ImagenesVehiculo)
            .Select(v => new
            {
                v.IdVehiculo,
                v.Marca,
                v.Modelo,
                v.Anio,
                v.Kilometraje,
                v.Potencia,
                v.Matricula,
                v.Color,
                v.NumeroPuertas,
                v.Descripcion,
                v.TipoCarroceria,
                v.TipoMotor,
                v.Estado,
                imagenes = v.ImagenesVehiculo.Select(img => new
                {
                    img.IdImagen,
                    img.Nombre,
                    img.Ruta,
                    img.Activo
                }).ToList()
            })
            .ToListAsync();

        return Ok(vehiculos);
    }

    /// <summary>
    /// Obtiene un vehículo por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetVehiculo(int id)
    {
        var vehiculo = await _context.Vehiculos
            .Include(v => v.ImagenesVehiculo)
            .Where(v => v.IdVehiculo == id)
            .Select(v => new
            {
                v.IdVehiculo,
                v.Marca,
                v.Modelo,
                v.Anio,
                v.Kilometraje,
                v.Potencia,
                v.Matricula,
                v.Color,
                v.NumeroPuertas,
                v.Descripcion,
                v.TipoCarroceria,
                v.TipoMotor,
                v.Estado,
                imagenes = v.ImagenesVehiculo.Select(img => new
                {
                    img.IdImagen,
                    img.Nombre,
                    img.Ruta,
                    img.Activo
                }).ToList()
            })
            .FirstOrDefaultAsync();

        return vehiculo == null ? NotFound() : Ok(vehiculo);
    }

    /// <summary>
    /// Crea un nuevo vehículo con sus imágenes.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<object>> PostVehiculo([FromBody] VehiculoConImagenesRequest request)
    {
        var vehiculo = new Vehiculo
        {
            Marca = request.Marca,
            Modelo = request.Modelo,
            Anio = request.Anio,
            Kilometraje = request.Kilometraje,
            Potencia = request.Potencia,
            Matricula = request.Matricula,
            Color = request.Color,
            NumeroPuertas = request.NumeroPuertas,
            Descripcion = request.Descripcion,
            TipoCarroceria = request.TipoCarroceria,
            TipoMotor = request.TipoMotor,
            Estado = request.Estado ?? "disponible"
        };

        _context.Vehiculos.Add(vehiculo);
        await _context.SaveChangesAsync();

        if (request.Imagenes != null && request.Imagenes.Any())
        {
            var carpeta = Path.Combine(_env.WebRootPath, "vehiculos", vehiculo.IdVehiculo.ToString());
            Directory.CreateDirectory(carpeta);

            foreach (var img in request.Imagenes)
            {
                try
                {
                    if (string.IsNullOrEmpty(img.ImagenBase64)) continue;

                    var bytes = Convert.FromBase64String(img.ImagenBase64);
                    var ext = DeterminarExtension(bytes);
                    var nombre = $"{Guid.NewGuid()}{ext}";
                    var rutaFisica = Path.Combine(carpeta, nombre);
                    var rutaRel = $"/vehiculos/{vehiculo.IdVehiculo}/{nombre}";

                    await System.IO.File.WriteAllBytesAsync(rutaFisica, bytes);

                    _context.ImagenesVehiculo.Add(new ImagenVehiculo
                    {
                        IdVehiculo = vehiculo.IdVehiculo,
                        Nombre = img.Nombre ?? nombre,
                        Ruta = rutaRel,
                        Activo = 1
                    });
                }
                catch { continue; }
            }

            await _context.SaveChangesAsync();
        }

        var resultado = await _context.Vehiculos
            .Include(v => v.ImagenesVehiculo)
            .Where(v => v.IdVehiculo == vehiculo.IdVehiculo)
            .Select(v => new
            {
                v.IdVehiculo,
                v.Marca,
                v.Modelo,
                v.Anio,
                imagenes = v.ImagenesVehiculo.Select(i => new { i.IdImagen, i.Ruta }).ToList()
            })
            .FirstOrDefaultAsync();

        return CreatedAtAction(nameof(GetVehiculo), new { id = vehiculo.IdVehiculo }, resultado);
    }

    /// <summary>
    /// Actualiza un vehículo existente.
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> PutVehiculo(int id, [FromBody] VehiculoConImagenesRequest request)
    {
        var vehiculo = await _context.Vehiculos.FindAsync(id);
        if (vehiculo == null) return NotFound();

        vehiculo.Marca = request.Marca;
        vehiculo.Modelo = request.Modelo;
        vehiculo.Anio = request.Anio;
        vehiculo.Kilometraje = request.Kilometraje;
        vehiculo.Potencia = request.Potencia;
        vehiculo.Matricula = request.Matricula;
        vehiculo.Color = request.Color;
        vehiculo.NumeroPuertas = request.NumeroPuertas;
        vehiculo.Descripcion = request.Descripcion;
        vehiculo.TipoCarroceria = request.TipoCarroceria;
        vehiculo.TipoMotor = request.TipoMotor;
        if (!string.IsNullOrEmpty(request.Estado)) vehiculo.Estado = request.Estado;

        _context.Entry(vehiculo).State = EntityState.Modified;

        if (request.Imagenes != null && request.Imagenes.Any())
        {
            var carpeta = Path.Combine(_env.WebRootPath, "vehiculos", id.ToString());
            Directory.CreateDirectory(carpeta);

            foreach (var img in request.Imagenes.Where(i => !i.IdImagen.HasValue))
            {
                try
                {
                    if (string.IsNullOrEmpty(img.ImagenBase64)) continue;

                    var bytes = Convert.FromBase64String(img.ImagenBase64);
                    var ext = DeterminarExtension(bytes);
                    var nombre = $"{Guid.NewGuid()}{ext}";
                    var rutaFisica = Path.Combine(carpeta, nombre);
                    var rutaRel = $"/vehiculos/{id}/{nombre}";

                    await System.IO.File.WriteAllBytesAsync(rutaFisica, bytes);

                    _context.ImagenesVehiculo.Add(new ImagenVehiculo
                    {
                        IdVehiculo = id,
                        Nombre = img.Nombre ?? nombre,
                        Ruta = rutaRel,
                        Activo = 1
                    });
                }
                catch { continue; }
            }
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Elimina un vehículo y sus imágenes.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteVehiculo(int id)
    {
        var vehiculo = await _context.Vehiculos
            .Include(v => v.ImagenesVehiculo)
            .FirstOrDefaultAsync(v => v.IdVehiculo == id);

        if (vehiculo == null) return NotFound();

        var carpeta = Path.Combine(_env.WebRootPath, "vehiculos", id.ToString());
        if (Directory.Exists(carpeta))
        {
            try { Directory.Delete(carpeta, true); } catch { }
        }

        _context.Vehiculos.Remove(vehiculo);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    private string DeterminarExtension(byte[] bytes)
    {
        if (bytes.Length >= 2)
        {
            if (bytes[0] == 0xFF && bytes[1] == 0xD8) return ".jpg";
            if (bytes[0] == 0x89 && bytes[1] == 0x50) return ".png";
            if (bytes[0] == 0x47 && bytes[1] == 0x49) return ".gif";
            if (bytes[0] == 0x42 && bytes[1] == 0x4D) return ".bmp";
        }
        return ".jpg";
    }
}

/// <summary>
/// Request para crear/actualizar vehículos con imágenes.
/// </summary>
public class VehiculoConImagenesRequest
{
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public int Anio { get; set; }
    public int Kilometraje { get; set; }
    public int Potencia { get; set; }
    public string Matricula { get; set; } = null!;
    public string Color { get; set; } = null!;
    public int NumeroPuertas { get; set; }
    public string? Descripcion { get; set; }
    public string TipoCarroceria { get; set; } = null!;
    public string TipoMotor { get; set; } = null!;
    public string? Estado { get; set; }
    public List<ImagenVehiculoBase64>? Imagenes { get; set; }
}

/// <summary>
/// Representa una imagen en Base64 para un vehículo.
/// </summary>
public class ImagenVehiculoBase64
{
    public int? IdImagen { get; set; }
    public string? ImagenBase64 { get; set; }
    public string? Nombre { get; set; }
}
