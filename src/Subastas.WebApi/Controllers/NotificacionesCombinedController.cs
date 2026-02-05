using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Subastas.Application.Dtos;
using Subastas.Infrastructure.Data;

namespace Subastas.WebApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificacionesCombinedController : ControllerBase
{
 private readonly SubastaContext _context;
 private readonly ILogger<NotificacionesCombinedController> _logger;

 public NotificacionesCombinedController(SubastaContext context, ILogger<NotificacionesCombinedController> logger)
 {
 _context = context;
 _logger = logger;
 }

 [HttpGet("todas")]
 public async Task<IActionResult> GetAllNotifications(int? usuarioId = null, bool? esAdmin = null, bool soloNoLeidas = false)
 {
 try
 {
 var userNotifsQuery = _context.Notificaciones
 .Include(n => n.Usuario)
 .Include(n => n.Subasta)
 .AsQueryable();

 if (usuarioId.HasValue)
 userNotifsQuery = userNotifsQuery.Where(n => n.IdUsuario == usuarioId.Value);
 if (soloNoLeidas)
 userNotifsQuery = userNotifsQuery.Where(n => n.Leida ==0);

 var userNotifs = await userNotifsQuery
 .Select(n => new NotificationDto
 {
 Id = n.IdNotificacion,
 Tipo = "usuario",
 Titulo = null,
 Mensaje = n.Mensaje,
 Fecha = n.FechaEnvio,
 Leida = n.Leida ==1,
 UsuarioId = n.IdUsuario,
 UsuarioNombre = n.Usuario != null ? n.Usuario.Nombre : null,
 SubastaId = n.IdSubasta,
 EsAdmin = false
 })
 .ToListAsync();

 var adminNotifsQuery = _context.NotificacionesAdmin
 .Include(a => a.Usuario)
 .AsQueryable();

 if (usuarioId.HasValue)
 adminNotifsQuery = adminNotifsQuery.Where(a => a.IdUsuario == usuarioId.Value);
 if (soloNoLeidas)
 adminNotifsQuery = adminNotifsQuery.Where(a => a.Leida ==0);

 var adminNotifs = await adminNotifsQuery
 .Select(a => new NotificationDto
 {
 Id = a.IdNotificacion,
 Tipo = a.Tipo,
 Titulo = a.Titulo,
 Mensaje = a.Mensaje,
 Fecha = a.FechaCreacion,
 Leida = a.Leida ==1,
 UsuarioId = a.IdUsuario,
 UsuarioNombre = a.Usuario != null ? a.Usuario.Nombre : null,
 SubastaId = (int?)null,
 EsAdmin = true
 })
 .ToListAsync();

 var combined = userNotifs.Concat(adminNotifs).ToList();
 if (esAdmin.HasValue)
 combined = combined.Where(n => n.EsAdmin == esAdmin.Value).ToList();

 combined = combined.OrderByDescending(n => n.Fecha).ToList();

 return Ok(combined);
 }
 catch (System.Exception ex)
 {
 _logger.LogError(ex, "Error combinando notificaciones: {Message}", ex.Message);
 return StatusCode(500, new { Success = false, Message = "Error al obtener notificaciones" });
 }
 }

 [HttpGet("usuario/{usuarioId}")]
 public async Task<IActionResult> GetNotificationsForUser(int usuarioId, bool soloNoLeidas = false)
 {
 try
 {
 _logger.LogInformation("GetNotificationsForUser: usuarioId={UsuarioId} soloNoLeidas={SoloNoLeidas}", usuarioId, soloNoLeidas);

 var userNotifsQuery = _context.Notificaciones
 .Include(n => n.Usuario)
 .Include(n => n.Subasta)
 .Where(n => n.IdUsuario == usuarioId);

 if (soloNoLeidas) userNotifsQuery = userNotifsQuery.Where(n => n.Leida ==0);

 var userNotifs = await userNotifsQuery
 .Select(n => new NotificationDto
 {
 Id = n.IdNotificacion,
 Tipo = "usuario",
 Titulo = null,
 Mensaje = n.Mensaje,
 Fecha = n.FechaEnvio,
 Leida = n.Leida ==1,
 UsuarioId = n.IdUsuario,
 UsuarioNombre = n.Usuario != null ? n.Usuario.Nombre : null,
 SubastaId = n.IdSubasta,
 EsAdmin = false
 })
 .ToListAsync();

 var adminNotifsQuery = _context.NotificacionesAdmin
 .Include(a => a.Usuario)
 .Where(a => a.IdUsuario == usuarioId);

 if (soloNoLeidas) adminNotifsQuery = adminNotifsQuery.Where(a => a.Leida ==0);

 var adminNotifs = await adminNotifsQuery
 .Select(a => new NotificationDto
 {
 Id = a.IdNotificacion,
 Tipo = a.Tipo,
 Titulo = a.Titulo,
 Mensaje = a.Mensaje,
 Fecha = a.FechaCreacion,
 Leida = a.Leida ==1,
 UsuarioId = a.IdUsuario,
 UsuarioNombre = a.Usuario != null ? a.Usuario.Nombre : null,
 SubastaId = (int?)null,
 EsAdmin = true
 })
 .ToListAsync();

 var combined = userNotifs.Concat(adminNotifs).OrderByDescending(n => n.Fecha).ToList();
 _logger.LogInformation("GetNotificationsForUser: found {Count} notifications for user {UsuarioId}", combined.Count, usuarioId);
 return Ok(combined);
 }
 catch (System.Exception ex)
 {
 _logger.LogError(ex, "Error obteniendo notificaciones por usuario: {Message}", ex.Message);
 return StatusCode(500, new { Success = false, Message = "Error al obtener notificaciones del usuario" });
 }
 }

 /// <summary>
 /// Marca una notificación como leída sin eliminarla.
 /// </summary>
 [HttpPatch("marcar-leida/{id}")]
 public async Task<IActionResult> MarkAsRead(int id)
 {
 try
 {
 // Try user notifications first
 var notif = await _context.Notificaciones.FirstOrDefaultAsync(n => n.IdNotificacion == id);
 if (notif != null)
 {
 if (notif.Leida ==1)
 {
 return Ok(new { Success = true, Message = "Notificación ya marcada como leída" });
 }

 notif.Leida =1;
 await _context.SaveChangesAsync();

 var dto = new NotificationDto
 {
 Id = notif.IdNotificacion,
 Tipo = "usuario",
 Titulo = null,
 Mensaje = notif.Mensaje,
 Fecha = notif.FechaEnvio,
 Leida = true,
 UsuarioId = notif.IdUsuario,
 UsuarioNombre = (await _context.Usuarios.FindAsync(notif.IdUsuario))?.Nombre,
 SubastaId = notif.IdSubasta,
 EsAdmin = false
 };

 return Ok(dto);
 }

 // Try admin notifications
 var admin = await _context.NotificacionesAdmin.FirstOrDefaultAsync(n => n.IdNotificacion == id);
 if (admin != null)
 {
 if (admin.Leida ==1)
 {
 return Ok(new { Success = true, Message = "Notificación admin ya marcada como leída" });
 }

 admin.Leida =1;
 await _context.SaveChangesAsync();

 var dto = new NotificationDto
 {
 Id = admin.IdNotificacion,
 Tipo = admin.Tipo,
 Titulo = admin.Titulo,
 Mensaje = admin.Mensaje,
 Fecha = admin.FechaCreacion,
 Leida = true,
 UsuarioId = admin.IdUsuario,
 UsuarioNombre = (await _context.Usuarios.FindAsync(admin.IdUsuario))?.Nombre,
 SubastaId = null,
 EsAdmin = true
 };

 return Ok(dto);
 }

 return NotFound(new { Success = false, Message = "Notificación no encontrada" });
 }
 catch (System.Exception ex)
 {
 _logger.LogError(ex, "Error marcando notificación como leída: {Message}", ex.Message);
 return StatusCode(500, new { Success = false, Message = "Error al marcar notificación como leída" });
 }
 }
}
