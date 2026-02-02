using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Net.Mail;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;
using Subastas.Application.Interfaces.Services;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para gestión de notificaciones y envío de emails.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class NotificacionesController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly IConfiguration _configuration;
    private readonly IEmailService _emailService;

    public NotificacionesController(SubastaContext context, IConfiguration configuration, IEmailService emailService)
    {
        _context = context;
        _configuration = configuration;
        _emailService = emailService;
    }

    /// <summary>
    /// Envía un email genérico.
    /// </summary>
    [HttpPost("enviar-email")]
    public async Task<IActionResult> EnviarEmail([FromBody] EmailRequest request)
    {
        try
        {
            var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"] ?? "";
            var smtpPass = _configuration["Email:SmtpPass"] ?? "";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(smtpUser, "Desguaces Borox"),
                Subject = request.Asunto,
                Body = request.Cuerpo,
                IsBodyHtml = true
            };

            mailMessage.To.Add(request.Destinatario);

            await Task.Run(() => client.Send(mailMessage));

            return Ok(new { mensaje = "Email enviado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { mensaje = "Error al enviar email.", error = ex.Message });
        }
    }

    /// <summary>
    /// Procesa subastas finalizadas, crea notificaciones para ganadores y perdedores.
    /// </summary>
    [HttpPost("procesar-finalizadas")]
    public async Task<IActionResult> ProcesarSubastasFinalizadas()
    {
        var ahora = DateTime.Now;
        var subastasFinalizadas = await _context.Subastas
            .Where(s => s.FechaFin <= ahora && s.Estado == "activa")
            .Include(s => s.Pujas)
            .ToListAsync();

        foreach (var subasta in subastasFinalizadas)
        {
            if (subasta.Pujas == null || !subasta.Pujas.Any())
                continue;

            var pujaGanadora = subasta.Pujas.OrderByDescending(p => p.Cantidad).FirstOrDefault();
            if (pujaGanadora != null)
            {
                // Obtener datos del ganador
                var ganador = await _context.Usuarios.FindAsync(pujaGanadora.IdUsuario);
                var vehiculo = await _context.Vehiculos.FindAsync(subasta.IdVehiculo);
                
                var mensajeGanador = $"¡Has ganado la subasta del vehículo {subasta.IdVehiculo}!";
                
                _context.Notificaciones.Add(new Notificacion
                {
                    IdUsuario = pujaGanadora.IdUsuario,
                    IdSubasta = subasta.IdSubasta,
                    Mensaje = mensajeGanador,
                    FechaEnvio = DateTime.Now,
                    Leida = 0
                });
                
                // Enviar email al ganador
                if (ganador != null && !string.IsNullOrEmpty(ganador.Email))
                {
                    var nombreGanador = $"{ganador.Nombre} {ganador.Apellidos}".Trim();
                    var vehiculoInfo = vehiculo != null ? $"{vehiculo.Marca} {vehiculo.Modelo} ({vehiculo.Anio})" : $"Subasta #{subasta.IdSubasta}";
                    await _emailService.EnviarEmailUsuarioAsync(
                        ganador.Email,
                        nombreGanador,
                        "¡Felicidades! Has ganado una subasta",
                        $"¡Felicidades {nombreGanador}!<br><br>Has ganado la subasta del vehículo <strong>{vehiculoInfo}</strong> con una puja de <strong>${pujaGanadora.Cantidad:N2}</strong>.<br><br>En breve nos pondremos en contacto contigo para coordinar la entrega del vehículo.");
                }

                var perdedores = subasta.Pujas
                    .Where(p => p.IdUsuario != pujaGanadora.IdUsuario)
                    .Select(p => p.IdUsuario)
                    .Distinct();

                foreach (var idPerdedor in perdedores)
                {
                    var perdedor = await _context.Usuarios.FindAsync(idPerdedor);
                    var vehiculoPerdedor = await _context.Vehiculos.FindAsync(subasta.IdVehiculo);
                    
                    var mensajePerdedor = $"No has ganado la subasta del vehículo {subasta.IdVehiculo}.";
                    
                    _context.Notificaciones.Add(new Notificacion
                    {
                        IdUsuario = idPerdedor,
                        IdSubasta = subasta.IdSubasta,
                        Mensaje = mensajePerdedor,
                        FechaEnvio = DateTime.Now,
                        Leida = 0
                    });
                    
                    // Enviar email al perdedor
                    if (perdedor != null && !string.IsNullOrEmpty(perdedor.Email))
                    {
                        var nombrePerdedor = $"{perdedor.Nombre} {perdedor.Apellidos}".Trim();
                        var vehiculoInfo = vehiculoPerdedor != null ? $"{vehiculoPerdedor.Marca} {vehiculoPerdedor.Modelo} ({vehiculoPerdedor.Anio})" : $"Subasta #{subasta.IdSubasta}";
                        await _emailService.EnviarEmailUsuarioAsync(
                            perdedor.Email,
                            nombrePerdedor,
                            "Subasta finalizada",
                            $"Hola {nombrePerdedor},<br><br>Lamentablemente no has ganado la subasta del vehículo <strong>{vehiculoInfo}</strong>.<br><br>Te invitamos a participar en nuestras próximas subastas. ¡No te pierdas las nuevas oportunidades!");
                    }
                }
            }

            subasta.Estado = "finalizada";
        }

        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Obtiene todas las notificaciones del usuario con información del vehículo.
    /// </summary>
    [HttpGet("{idUsuario}")]
    public async Task<ActionResult<IEnumerable<object>>> GetNotificacionesUsuario(int idUsuario)
    {
        try
        {
            var notificaciones = await _context.Notificaciones
                .Where(n => n.IdUsuario == idUsuario)
                .Include(n => n.Subasta)
                    .ThenInclude(s => s.Vehiculo)
                .OrderByDescending(n => n.FechaEnvio)
            .Select(n => new
            {
                idNotificacion = n.IdNotificacion,
                idUsuario = n.IdUsuario,
                idSubasta = n.IdSubasta,
                mensaje = n.Mensaje,
                fechaCreacion = n.FechaEnvio,
                leida = n.Leida == 1,
                vehiculo = n.Subasta != null && n.Subasta.Vehiculo != null
                    ? new
                    {
                        marca = n.Subasta.Vehiculo.Marca,
                        modelo = n.Subasta.Vehiculo.Modelo,
                        anio = n.Subasta.Vehiculo.Anio
                    }
                    : null
            })
            .ToListAsync();

            return Ok(notificaciones);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Error al cargar notificaciones", Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Marca una notificación como leída.
    /// </summary>
    [HttpPut("{id}/leida")]
    public async Task<IActionResult> MarcarComoLeida(int id)
    {
        var notificacion = await _context.Notificaciones.FindAsync(id);
        if (notificacion == null) return NotFound();

        notificacion.Leida = 1;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    /// <summary>
    /// Marca todas las notificaciones de un usuario como leídas.
    /// </summary>
    [HttpPut("usuario/{idUsuario}/leidas")]
    public async Task<IActionResult> MarcarTodasComoLeidas(int idUsuario)
    {
        var notificaciones = await _context.Notificaciones
            .Where(n => n.IdUsuario == idUsuario && n.Leida == 0)
            .ToListAsync();

        if (!notificaciones.Any())
            return NoContent();

        foreach (var n in notificaciones)
            n.Leida = 1;

        await _context.SaveChangesAsync();
        return NoContent();
    }
}

/// <summary>
/// Modelo para envío de emails.
/// </summary>
public class EmailRequest
{
    public string Destinatario { get; set; } = null!;
    public string Asunto { get; set; } = null!;
    public string Cuerpo { get; set; } = null!;
}
