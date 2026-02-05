using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;
using Subastas.Application.Interfaces.Services;

namespace Subastas.WebApi.Services
{
 /// <summary>
 /// Background service that periodically checks for auctions that have ended
 /// and finalizes them (determines winner, notifies users and admins).
 /// </summary>
 public class AuctionFinalizerService : BackgroundService
 {
 private readonly IServiceScopeFactory _scopeFactory;
 private readonly ILogger<AuctionFinalizerService> _logger;
 private readonly TimeSpan _period;

 public AuctionFinalizerService(IServiceScopeFactory scopeFactory, ILogger<AuctionFinalizerService> logger)
 {
 _scopeFactory = scopeFactory;
 _logger = logger;
 // Period can be made configurable; check every 30 seconds by default
 _period = TimeSpan.FromSeconds(30);
 }

 protected override async Task ExecuteAsync(CancellationToken stoppingToken)
 {
 _logger.LogInformation("AuctionFinalizerService started; checking every {Period}s.", _period.TotalSeconds);

 while (!stoppingToken.IsCancellationRequested)
 {
 try
 {
 await ProcessExpiredAuctions(stoppingToken);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error while processing expired auctions: {Message}", ex.Message);
 }

 await Task.Delay(_period, stoppingToken);
 }

 _logger.LogInformation("AuctionFinalizerService stopping.");
 }

 private async Task ProcessExpiredAuctions(CancellationToken ct)
 {
 using var scope = _scopeFactory.CreateScope();
 var db = scope.ServiceProvider.GetRequiredService<SubastaContext>();
 var email = scope.ServiceProvider.GetService<IEmailService>();

 // Use DateTime.UtcNow to match UTC if FechaFin is stored in UTC
 var ahora = DateTime.UtcNow;

 // Select auctions that are not finalized and whose FechaFin <= now
 var expired = await db.Subastas
 .Where(s => s.Estado != "finalizada" && s.FechaFin <= ahora)
 .Include(s => s.Vehiculo)
 .ToListAsync(ct);

 if (!expired.Any())
 {
 _logger.LogDebug("No expired auctions found at {Time}.", ahora);
 return;
 }

 _logger.LogInformation("Found {Count} expired auctions to finalize.", expired.Count);

 foreach (var subasta in expired)
 {
 try
 {
 _logger.LogInformation("Finalizing subasta {Id} (FechaFin {FechaFin}).", subasta.IdSubasta, subasta.FechaFin);

 // Get all bids for the auction
 var pujas = await db.Pujas
 .Where(p => p.IdSubasta == subasta.IdSubasta)
 .OrderByDescending(p => p.Cantidad)
 .ToListAsync(ct);

 if (!pujas.Any())
 {
 subasta.Estado = "finalizada";
 await db.SaveChangesAsync(ct);
 _logger.LogInformation("Subasta {Id} finalized with no bids.", subasta.IdSubasta);
 continue;
 }

 var winner = pujas.First();
 var losers = pujas.Where(p => p.IdUsuario != winner.IdUsuario).Select(p => p.IdUsuario).Distinct().ToList();

 subasta.PrecioActual = winner.Cantidad;
 subasta.Estado = "finalizada";

 // Notify winner
 var winnerUser = await db.Usuarios.FindAsync(new object[] { winner.IdUsuario }, ct);
 if (winnerUser != null)
 {
 var msg = $"¡Has ganado la subasta #{subasta.IdSubasta} con {winner.Cantidad:C}! En 30 días máximo, el administrador te contactará para ultimar los detalles de la venta.";
 db.Notificaciones.Add(new Notificacion
 {
 IdUsuario = winnerUser.IdUsuario,
 IdSubasta = subasta.IdSubasta,
 Mensaje = msg,
 FechaEnvio = DateTime.UtcNow,
 Leida =0
 });

 try { if (email != null) await email.EnviarEmailUsuarioAsync(winnerUser.Email, winnerUser.Nombre, "Has ganado la subasta", msg); } catch (Exception ex) { _logger.LogWarning(ex, "Failed sending winner email for subasta {Id}", subasta.IdSubasta); }
 }

 // Notify losers
 foreach (var loserId in losers)
 {
 var loser = await db.Usuarios.FindAsync(new object[] { loserId }, ct);
 if (loser == null) continue;
 var msg = $"No has ganado la subasta #{subasta.IdSubasta}. Puja ganadora: {winner.Cantidad:C}.";
 db.Notificaciones.Add(new Notificacion
 {
 IdUsuario = loser.IdUsuario,
 IdSubasta = subasta.IdSubasta,
 Mensaje = msg,
 FechaEnvio = DateTime.UtcNow,
 Leida =0
 });

 try { if (email != null) await email.EnviarEmailUsuarioAsync(loser.Email, loser.Nombre, "Subasta finalizada - resultado", msg); } catch (Exception ex) { _logger.LogWarning(ex, "Failed sending loser email for subasta {Id} to {Email}", subasta.IdSubasta, loser.Email); }
 }

 // Admin notification
 db.NotificacionesAdmin.Add(new NotificacionAdmin
 {
 Titulo = "Subasta finalizada",
 Mensaje = $"Subasta #{subasta.IdSubasta} finalizada. Ganador: {(winnerUser?.Email ?? winner.IdUsuario.ToString())} - {winner.Cantidad:C}",
 Tipo = "finalizacion",
 IdUsuario = null, // Set to null so it's visible to all admins
 Leida =0,
 FechaCreacion = DateTime.UtcNow
 });

 await db.SaveChangesAsync(ct);
 _logger.LogInformation("Subasta {Id} finalized; winner {WinnerId}; notifications created.", subasta.IdSubasta, winner.IdUsuario);
 }
 catch (Exception ex)
 {
 _logger.LogError(ex, "Error finalizing subasta {Id}: {Message}", subasta.IdSubasta, ex.Message);
 }
 }
 }
 }
}
