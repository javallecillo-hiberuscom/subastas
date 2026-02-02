using Subastas.Domain.Entities;

namespace Subastas.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de notificaciones administrativas.
/// </summary>
public interface INotificacionAdminService
{
    /// <summary>
    /// Crea una notificación de registro de usuario.
    /// </summary>
    Task CrearNotificacionRegistroAsync(int idUsuario, string nombreUsuario, string email);

    /// <summary>
    /// Crea una notificación de documento subido.
    /// </summary>
    Task CrearNotificacionDocumentoSubidoAsync(int idUsuario, string nombreUsuario, string tipoDocumento);

    /// <summary>
    /// Crea una notificación de nueva puja.
    /// </summary>
    Task CrearNotificacionPujaAsync(int idUsuario, int idSubasta, decimal cantidad);

    /// <summary>
    /// Obtiene todas las notificaciones no leídas.
    /// </summary>
    Task<IEnumerable<NotificacionAdmin>> ObtenerNotificacionesNoLeidasAsync();

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    Task MarcarComoLeidaAsync(int idNotificacion);
}
