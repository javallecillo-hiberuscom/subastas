using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Subastas.Application.Interfaces.Services;

namespace Subastas.Infrastructure.Services;

/// <summary>
/// Servicio para envío de emails usando SMTP.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Envía un email genérico.
    /// </summary>
    public async Task<bool> EnviarEmailAsync(string destinatario, string asunto, string cuerpo)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"] ?? "";
            var smtpPass = _configuration["Email:SmtpPass"] ?? "";
            var fromName = _configuration["Email:FromName"] ?? "Sistema de Subastas";

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                _logger.LogWarning("Configuración de email no encontrada. Email no enviado.");
                return false;
            }

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, fromName),
                Subject = asunto,
                Body = cuerpo,
                IsBodyHtml = true
            };

            mailMessage.To.Add(destinatario);

            await Task.Run(() => client.Send(mailMessage));
            
            _logger.LogInformation($"Email enviado correctamente a {destinatario}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar email a {destinatario}");
            return false;
        }
    }

    /// <summary>
    /// Envía email de notificación a administradores.
    /// </summary>
    public async Task EnviarEmailAdminAsync(string titulo, string mensaje)
    {
        var emailAdmin = _configuration["Email:AdminEmail"];
        if (string.IsNullOrEmpty(emailAdmin))
        {
            _logger.LogWarning("Email de administrador no configurado.");
            return;
        }

        var cuerpo = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ padding: 20px; background-color: #f5f5f5; }}
                    .content {{ background-color: white; padding: 20px; border-radius: 8px; }}
                    .header {{ background-color: #2563eb; color: white; padding: 15px; border-radius: 8px 8px 0 0; }}
                    .message {{ padding: 20px; line-height: 1.6; }}
                    .footer {{ text-align: center; padding: 15px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='content'>
                        <div class='header'>
                            <h2>🔔 {titulo}</h2>
                        </div>
                        <div class='message'>
                            <p>{mensaje}</p>
                            <p><a href='#' style='background-color: #2563eb; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px;'>Ver Panel de Administración</a></p>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Subastas - Desguaces Borox</p>
                            <p>Este es un mensaje automático, por favor no responder.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";

        await EnviarEmailAsync(emailAdmin, titulo, cuerpo);
    }

    /// <summary>
    /// Envía email de notificación a un usuario específico.
    /// </summary>
    public async Task EnviarEmailUsuarioAsync(string emailUsuario, string nombreUsuario, string asunto, string mensaje)
    {
        var cuerpo = $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ padding: 20px; background-color: #f5f5f5; }}
                    .content {{ background-color: white; padding: 20px; border-radius: 8px; }}
                    .header {{ background-color: #10b981; color: white; padding: 15px; border-radius: 8px 8px 0 0; }}
                    .message {{ padding: 20px; line-height: 1.6; }}
                    .footer {{ text-align: center; padding: 15px; color: #666; font-size: 12px; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='content'>
                        <div class='header'>
                            <h2>🔔 {asunto}</h2>
                        </div>
                        <div class='message'>
                            <p>Hola <strong>{nombreUsuario}</strong>,</p>
                            <p>{mensaje}</p>
                            <p><a href='#' style='background-color: #10b981; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px; display: inline-block; margin-top: 10px;'>Ver Mis Notificaciones</a></p>
                        </div>
                        <div class='footer'>
                            <p>Sistema de Subastas - Desguaces Borox</p>
                            <p>Este es un mensaje automático, por favor no responder.</p>
                        </div>
                    </div>
                </div>
            </body>
            </html>";

        await EnviarEmailAsync(emailUsuario, asunto, cuerpo);
    }
}
