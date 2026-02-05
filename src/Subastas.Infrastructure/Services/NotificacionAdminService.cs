using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    private readonly ILogger<NotificacionAdminService> _logger;

    public NotificacionAdminService(
        SubastaContext context,
        IEmailService emailService,
        ILogger<NotificacionAdminService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        _logger.LogWarning(">>> [{Timestamp}] NotificacionAdminService CONSTRUCTOR <<<", timestamp);
    }

    /// <summary>
    /// Crea una notificación cuando se registra un nuevo usuario.
    /// </summary>
    public async Task CrearNotificacionRegistroAsync(int idUsuario, string nombreUsuario, string email)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
        try
        {
            _logger.LogWarning(">>> [{Timestamp}] PUNTO A: INICIO CrearNotificacionRegistroAsync ID={IdUsuario}", timestamp, idUsuario);
            
            var notificacion = new NotificacionAdmin
            {
                Titulo = "Nuevo usuario registrado",
                Mensaje = $"El usuario {nombreUsuario} ({email}) se ha registrado y está pendiente de validación.",
                Tipo = "registro",
                IdUsuario = idUsuario,
                Leida = 0,
                FechaCreacion = DateTime.Now
            };

            _logger.LogWarning(">>> [{Timestamp}] PUNTO B: Agregando notificación...", DateTime.Now.ToString("HH:mm:ss.fff"));
            _context.NotificacionesAdmin.Add(notificacion);
            
            _logger.LogWarning(">>> [{Timestamp}] PUNTO C: Guardando en BD...", DateTime.Now.ToString("HH:mm:ss.fff"));
            var rowsAffected = await _context.SaveChangesAsync();
            _logger.LogWarning(">>> [{Timestamp}] PUNTO D: Filas={Rows}, IdNotif={IdNotificacion}", DateTime.Now.ToString("HH:mm:ss.fff"), rowsAffected, notificacion.IdNotificacion);
            
            // Enviar email al administrador
            try
            {
                _logger.LogWarning(">>> [{Timestamp}] PUNTO E: Enviando email...", DateTime.Now.ToString("HH:mm:ss.fff"));
                await _emailService.EnviarEmailAdminAsync(
                    notificacion.Titulo,
                    notificacion.Mensaje);
                _logger.LogWarning(">>> [{Timestamp}] PUNTO F: Email enviado EXITOSAMENTE", DateTime.Now.ToString("HH:mm:ss.fff"));
            }
            catch (Exception emailEx)
            {
                _logger.LogWarning(emailEx, ">>> [{Timestamp}] Email falló pero notificación guardada", DateTime.Now.ToString("HH:mm:ss.fff"));
            }
            
            _logger.LogWarning(">>> [{Timestamp}] PUNTO I: FIN - ÉXITO TOTAL", DateTime.Now.ToString("HH:mm:ss.fff"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ">>> [{Timestamp}] ERROR CRÍTICO: {Message}", DateTime.Now.ToString("HH:mm:ss.fff"), ex.Message);
            _logger.LogError("StackTrace: {StackTrace}", ex.StackTrace);
            if (ex.InnerException != null)
            {
                _logger.LogError("InnerException: {InnerMessage}", ex.InnerException.Message);
            }
            throw;
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
