using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;
using Subastas.Application.Interfaces.Services;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador para gestión de documentos IAE de usuarios.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class DocumentosController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly IWebHostEnvironment _env;
    private readonly IEmailService _emailService;

    public DocumentosController(SubastaContext context, IWebHostEnvironment env, IEmailService emailService)
    {
        _context = context;
        _env = env;
        _emailService = emailService;
    }

    /// <summary>
    /// Sube el documento IAE de un usuario.
    /// </summary>
    [HttpPost("subir-iae/{idUsuario}")]
    public async Task<IActionResult> SubirIAE(int idUsuario, [FromBody] SubirDocumentoRequest request)
    {
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        try
        {
            // Validar que sea un archivo válido (Base64)
            byte[] documentoBytes = Convert.FromBase64String(request.DocumentoBase64);

            // Validar tamaño máximo (10MB)
            if (documentoBytes.Length > 10 * 1024 * 1024)
                return BadRequest("El documento no puede superar los 10MB.");

            // Crear directorio si no existe
            var uploadsPath = Path.Combine(_env.ContentRootPath, "Uploads", "IAE");
            if (!Directory.Exists(uploadsPath))
                Directory.CreateDirectory(uploadsPath);

            // Generar nombre único para el archivo
            var extension = request.NombreArchivo.Contains('.') 
                ? Path.GetExtension(request.NombreArchivo) 
                : ".pdf";
            var nombreArchivo = $"IAE_Usuario_{idUsuario}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var rutaCompleta = Path.Combine(uploadsPath, nombreArchivo);

            // Guardar archivo
            await System.IO.File.WriteAllBytesAsync(rutaCompleta, documentoBytes);

            // Actualizar usuario con ruta del documento (ruta compatible con frontend)
            usuario.DocumentoIAE = $"uploads/IAE/{nombreArchivo}";
            
            // Crear notificación para administradores
            var notificacion = new NotificacionAdmin
            {
                Titulo = "Nuevo documento IAE subido",
                Mensaje = $"{usuario.Nombre} {usuario.Apellidos} ha subido su documento IAE y está pendiente de validación.",
                Tipo = "documento_subido",
                IdUsuario = usuario.IdUsuario,
                FechaCreacion = DateTime.Now,
                Leida = 0
            };
            _context.NotificacionesAdmin.Add(notificacion);
            
            // Enviar email al administrador
            await _emailService.EnviarEmailAdminAsync(
                notificacion.Titulo,
                notificacion.Mensaje);
            
            await _context.SaveChangesAsync();

            return Ok(new
            {
                mensaje = "Documento IAE subido correctamente. Pendiente de validación por un administrador.",
                ruta = usuario.DocumentoIAE
            });
        }
        catch (FormatException)
        {
            return BadRequest("Formato de documento Base64 inválido.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al subir el documento.", error = ex.Message });
        }
    }

    /// <summary>
    /// Descarga el documento IAE de un usuario.
    /// </summary>
    [HttpGet("descargar-iae/{idUsuario}")]
    public async Task<IActionResult> DescargarIAE(int idUsuario)
    {
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        if (string.IsNullOrEmpty(usuario.DocumentoIAE))
            return NotFound("El usuario no ha subido ningún documento IAE.");

        try
        {
            var rutaRelativa = usuario.DocumentoIAE.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var rutaCompleta = Path.Combine(_env.ContentRootPath, rutaRelativa);

            if (!System.IO.File.Exists(rutaCompleta))
                return NotFound("El archivo del documento no existe.");

            var bytes = await System.IO.File.ReadAllBytesAsync(rutaCompleta);
            var nombreArchivo = Path.GetFileName(rutaCompleta);

            return File(bytes, "application/pdf", nombreArchivo);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al descargar el documento.", error = ex.Message });
        }
    }

    /// <summary>
    /// Verifica si un usuario tiene documento IAE subido.
    /// </summary>
    [HttpGet("verificar-iae/{idUsuario}")]
    public async Task<IActionResult> VerificarIAE(int idUsuario)
    {
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        if (usuario == null)
            return NotFound("Usuario no encontrado.");

        return Ok(new
        {
            tieneDocumento = !string.IsNullOrEmpty(usuario.DocumentoIAE),
            validado = usuario.Validado == 1,
            rutaDocumento = usuario.DocumentoIAE
        });
    }
}

/// <summary>
/// Request para subir documentos.
/// </summary>
public class SubirDocumentoRequest
{
    public string DocumentoBase64 { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
}
