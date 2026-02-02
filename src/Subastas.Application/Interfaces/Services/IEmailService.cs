namespace Subastas.Application.Interfaces.Services;

/// <summary>
/// Servicio para envío de emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Envía un email genérico.
    /// </summary>
    Task<bool> EnviarEmailAsync(string destinatario, string asunto, string cuerpo);
    
    /// <summary>
    /// Envía email de notificación a administradores.
    /// </summary>
    Task EnviarEmailAdminAsync(string titulo, string mensaje);
    
    /// <summary>
    /// Envía email de notificación a un usuario específico.
    /// </summary>
    Task EnviarEmailUsuarioAsync(string emailUsuario, string nombreUsuario, string asunto, string mensaje);
}
