using Microsoft.EntityFrameworkCore;
using Subastas.Application.Interfaces.Services;
using Subastas.Domain.Entities;
using Subastas.Infrastructure.Data;

namespace Subastas.Infrastructure.Services;

/// <summary>
/// Servicio para gestionar notificaciones administrativas del sistema.
/// </summary>
public class NotificacionAdminService : INotificacionAdminService
{
    private readonly SubastaContext _context;
    private readonly IEmailService _emailService;

    public NotificacionAdminService(SubastaContext context, IEmailService emailService)
    {
        _context = context;
        _emailService = emailService;
    }

    /// <summary>
    /// Crea una notificación cuando se registra un nuevo usuario.
    /// </summary>
    public async Task CrearNotificacionRegistroAsync(int idUsuario, string nombreUsuario, string email)
    {
        var notificacion = new NotificacionAdmin
        {
            Titulo = "Nuevo usuario registrado",
            Mensaje = $"El usuario {nombreUsuario} ({email}) se ha registrado y está pendiente de validación.",
            Tipo = "registro",
            IdUsuario = idUsuario,
            Leida = 0,
            FechaCreacion = DateTime.Now
        };

        await _context.NotificacionesAdmin.AddAsync(notificacion);
        await _context.SaveChangesAsync();
        
        // Enviar email al administrador (no falla si el email no se puede enviar)
        try
        {
            await _emailService.EnviarEmailAdminAsync(
                notificacion.Titulo,
                notificacion.Mensaje);
        }
        catch
        {
            // Ignorar errores de email, la notificación ya está creada
        }
    }

    /// <summary>
    /// Crea una notificación cuando un usuario sube un documento.
    /// </summary>
    public async Task CrearNotificacionDocumentoSubidoAsync(int idUsuario, string nombreUsuario, string tipoDocumento)
    {
        var notificacion = new NotificacionAdmin
        {
            Titulo = $"Documento {tipoDocumento} subido",
            Mensaje = $"El usuario {nombreUsuario} ha subido su documento {tipoDocumento} y está pendiente de validación.",
            Tipo = "documento_subido",
            IdUsuario = idUsuario,
            Leida = 0,
            FechaCreacion = DateTime.Now
        };

        await _context.NotificacionesAdmin.AddAsync(notificacion);
        await _context.SaveChangesAsync();
        
        // Enviar email al administrador
        await _emailService.EnviarEmailAdminAsync(
            notificacion.Titulo,
            notificacion.Mensaje);
    }

    /// <summary>
    /// Crea una notificación cuando se realiza una nueva puja.
    /// </summary>
    public async Task CrearNotificacionPujaAsync(int idUsuario, int idSubasta, decimal cantidad)
    {
        // Obtener información del usuario y subasta
        var usuario = await _context.Usuarios.FindAsync(idUsuario);
        var subasta = await _context.Subastas
            .Include(s => s.Vehiculo)
            .FirstOrDefaultAsync(s => s.IdSubasta == idSubasta);

        if (usuario == null || subasta == null)
            return;

        var nombreUsuario = $"{usuario.Nombre} {usuario.Apellidos}".Trim();
        var vehiculo = $"{subasta.Vehiculo.Marca} {subasta.Vehiculo.Modelo}".Trim();

        var notificacion = new NotificacionAdmin
        {
            Titulo = "Nueva puja realizada",
            Mensaje = $"{nombreUsuario} ha realizado una puja de ${cantidad:N2} en {vehiculo} (Subasta #{idSubasta}).",
            Tipo = "puja",
            IdUsuario = idUsuario,
            Leida = 0,
            FechaCreacion = DateTime.Now,
            DatosAdicionales = $"{{\"idSubasta\": {idSubasta}, \"cantidad\": {cantidad}}}"
        };

        await _context.NotificacionesAdmin.AddAsync(notificacion);
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Obtiene todas las notificaciones no leídas, ordenadas por fecha.
    /// </summary>
    public async Task<IEnumerable<NotificacionAdmin>> ObtenerNotificacionesNoLeidasAsync()
    {
        return await _context.NotificacionesAdmin
            .Where(n => n.Leida == 0)
            .Include(n => n.Usuario)
            .OrderByDescending(n => n.FechaCreacion)
            .ToListAsync();
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    public async Task MarcarComoLeidaAsync(int idNotificacion)
    {
        var notificacion = await _context.NotificacionesAdmin.FindAsync(idNotificacion);
        if (notificacion != null)
        {
            notificacion.Leida = 1;
            await _context.SaveChangesAsync();
        }
    }
}
