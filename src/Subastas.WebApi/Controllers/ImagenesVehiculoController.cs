using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para gestión de imágenes de vehículos usando Base64.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class ImagenesVehiculoController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly IWebHostEnvironment _env;

    public ImagenesVehiculoController(SubastaContext context, IWebHostEnvironment env)
    {
        _context = context;
        _env = env;
    }

    /// <summary>
    /// Sube una imagen codificada en Base64 para un vehículo.
    /// </summary>
    /// <param name="request">Datos de la imagen en Base64.</param>
    [HttpPost]
    public async Task<IActionResult> PostImagen([FromBody] ImagenBase64Request request)
    {
        if (string.IsNullOrEmpty(request.ImagenBase64))
            return BadRequest("Imagen no válida.");

        var vehiculo = await _context.Vehiculos.FindAsync(request.IdVehiculo);
        if (vehiculo == null)
            return NotFound("Vehículo no encontrado.");

        try
        {
            // Decodificar Base64
            var imageBytes = Convert.FromBase64String(request.ImagenBase64);

            // Determinar extensión del archivo
            var extension = DeterminarExtension(imageBytes);
            
            // Carpeta destino: wwwroot/vehiculos/{idVehiculo}/
            var carpeta = Path.Combine(_env.WebRootPath, "vehiculos", request.IdVehiculo.ToString());
            Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{Guid.NewGuid()}{extension}";
            var rutaFisica = Path.Combine(carpeta, nombreArchivo);
            var rutaRelativa = $"/vehiculos/{request.IdVehiculo}/{nombreArchivo}";

            // Guardar archivo
            await System.IO.File.WriteAllBytesAsync(rutaFisica, imageBytes);

            var imagen = new ImagenVehiculo
            {
                IdVehiculo = request.IdVehiculo,
                Nombre = request.Nombre ?? nombreArchivo,
                Ruta = rutaRelativa,
                Activo = 1
            };

            _context.ImagenesVehiculo.Add(imagen);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                imagen.IdImagen,
                imagen.IdVehiculo,
                imagen.Nombre,
                imagen.Ruta,
                imagen.Activo
            });
        }
        catch (FormatException)
        {
            return BadRequest("Formato de imagen Base64 inválido.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al guardar la imagen: {ex.Message}");
        }
    }

    /// <summary>
    /// Obtiene todas las imágenes activas de un vehículo en Base64.
    /// </summary>
    [HttpGet("vehiculo/{idVehiculo}")]
    public async Task<ActionResult<IEnumerable<object>>> GetImagenesPorVehiculo(int idVehiculo)
    {
        var imagenes = await _context.ImagenesVehiculo
            .Where(img => img.IdVehiculo == idVehiculo && img.Activo == 1)
            .OrderBy(img => img.IdImagen)
            .ToListAsync();

        var imagenesBase64 = new List<object>();

        foreach (var img in imagenes)
        {
            try
            {
                if (string.IsNullOrEmpty(img.Ruta))
                {
                    imagenesBase64.Add(new { img.IdImagen, img.IdVehiculo, img.Nombre, img.Ruta, img.Activo, imagenBase64 = (string?)null });
                    continue;
                }
                
                var rutaFisica = Path.Combine(_env.WebRootPath, img.Ruta.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                
                if (System.IO.File.Exists(rutaFisica))
                {
                    var bytes = await System.IO.File.ReadAllBytesAsync(rutaFisica);
                    var base64 = Convert.ToBase64String(bytes);
                    
                    imagenesBase64.Add(new
                    {
                        img.IdImagen,
                        img.IdVehiculo,
                        img.Nombre,
                        img.Ruta,
                        img.Activo,
                        imagenBase64 = base64
                    });
                }
                else
                {
                    imagenesBase64.Add(new
                    {
                        img.IdImagen,
                        img.IdVehiculo,
                        img.Nombre,
                        img.Ruta,
                        img.Activo,
                        imagenBase64 = (string?)null
                    });
                }
            }
            catch
            {
                imagenesBase64.Add(new
                {
                    img.IdImagen,
                    img.IdVehiculo,
                    img.Nombre,
                    img.Ruta,
                    img.Activo,
                    imagenBase64 = (string?)null
                });
            }
        }

        return Ok(imagenesBase64);
    }

    /// <summary>
    /// Elimina una imagen físicamente y de la base de datos.
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteImagen(int id)
    {
        var imagen = await _context.ImagenesVehiculo.FindAsync(id);
        if (imagen == null)
            return NotFound();

        if (!string.IsNullOrEmpty(imagen.Ruta))
        {
            var rutaFisica = Path.Combine(_env.WebRootPath, imagen.Ruta.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            
            if (System.IO.File.Exists(rutaFisica))
                System.IO.File.Delete(rutaFisica);
        }

        _context.ImagenesVehiculo.Remove(imagen);
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Determina la extensión del archivo según los bytes de la imagen.
    /// </summary>
    private string DeterminarExtension(byte[] imageBytes)
    {
        if (imageBytes.Length >= 2)
        {
            // JPG/JPEG
            if (imageBytes[0] == 0xFF && imageBytes[1] == 0xD8)
                return ".jpg";
            
            // PNG
            if (imageBytes[0] == 0x89 && imageBytes[1] == 0x50)
                return ".png";
            
            // GIF
            if (imageBytes[0] == 0x47 && imageBytes[1] == 0x49)
                return ".gif";
            
            // BMP
            if (imageBytes[0] == 0x42 && imageBytes[1] == 0x4D)
                return ".bmp";
        }
        
        // Por defecto JPG
        return ".jpg";
    }
}

/// <summary>
/// Modelo para subir imagen en Base64.
/// </summary>
public class ImagenBase64Request
{
    public int IdVehiculo { get; set; }
    public string ImagenBase64 { get; set; } = null!;
    public string? Nombre { get; set; }
}
