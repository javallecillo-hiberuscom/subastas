using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador para gestión de notificaciones del administrador.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class NotificacionesAdminController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly ILogger<NotificacionesAdminController> _logger;

    public NotificacionesAdminController(SubastaContext context, ILogger<NotificacionesAdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene las notificaciones administrativas con opción de filtrar solo las no leídas.
    /// </summary>
    /// <param name="soloNoLeidas">Si es true, devuelve solo notificaciones no leídas.</param>
    /// <param name="limite">Número máximo de notificaciones a retornar.</param>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<NotificacionAdmin>>> GetNotificaciones(
        [FromQuery] bool? soloNoLeidas = null,
        [FromQuery] int? limite = 50)
    {
        try
        {
            var query = _context.NotificacionesAdmin
                .Include(n => n.Usuario)
                .AsQueryable();

            if (soloNoLeidas.HasValue && soloNoLeidas.Value)
            {
                query = query.Where(n => n.Leida == 0);
            }

            query = query.OrderByDescending(n => n.FechaCreacion)
                        .Take(limite ?? 50);

            var notificaciones = await query.ToListAsync();
            return Ok(notificaciones);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo notificaciones administrativas");
            // Retornar lista vacía en caso de error
            return Ok(new List<NotificacionAdmin>());
        }
    }

    /// <summary>
    /// Obtiene el contador de notificaciones no leídas.
    /// </summary>
    [HttpGet("contador-no-leidas")]
    public async Task<ActionResult<int>> GetContadorNoLeidas()
    {
        try
        {
            var contador = await _context.NotificacionesAdmin
                .CountAsync(n => n.Leida == 0);
            
            return Ok(new { contador });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo contador de notificaciones no leídas");
            // Retornar 0 en caso de error (tabla no existe, etc)
            return Ok(new { contador = 0 });
        }
    }

    /// <summary>
    /// Crea una nueva notificación administrativa.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<NotificacionAdmin>> CrearNotificacion(NotificacionAdmin notificacion)
    {
        notificacion.FechaCreacion = DateTime.Now;
        notificacion.Leida = 0;

        _context.NotificacionesAdmin.Add(notificacion);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetNotificaciones), new { id = notificacion.IdNotificacion }, notificacion);
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    /// <param name="id">ID de la notificación.</param>
    [HttpPut("{id}/marcar-leida")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var notificacion = await _context.NotificacionesAdmin.FindAsync(id);
        if (notificacion == null)
        {
            return NotFound();
        }

        notificacion.Leida = 1;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Marca todas las notificaciones como leídas.
    /// </summary>
    [HttpPut("marcar-todas-leidas")]
    public async Task<IActionResult> MarcarTodasComoLeidas()
    {
        await _context.NotificacionesAdmin
            .Where(n => n.Leida == 0)
            .ExecuteUpdateAsync(n => n.SetProperty(p => p.Leida, 1));

        return NoContent();
    }

    /// <summary>
    /// Elimina una notificación específica.
    /// </summary>
    /// <param name="id">ID de la notificación a eliminar.</param>
    [HttpDelete("{id}")]
    public async Task<IActionResult> EliminarNotificacion(int id)
    {
        var notificacion = await _context.NotificacionesAdmin.FindAsync(id);
        if (notificacion == null)
        {
            return NotFound();
        }

        _context.NotificacionesAdmin.Remove(notificacion);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>
    /// Elimina todas las notificaciones ya leídas.
    /// </summary>
    [HttpDelete("limpiar-leidas")]
    public async Task<IActionResult> LimpiarNotificacionesLeidas()
    {
        await _context.NotificacionesAdmin
            .Where(n => n.Leida == 1)
            .ExecuteDeleteAsync();

        return NoContent();
    }
}
