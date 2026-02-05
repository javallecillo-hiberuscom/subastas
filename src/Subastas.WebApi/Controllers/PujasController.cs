using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Subastas.Infrastructure.Data;
using Subastas.Domain.Entities;
using Subastas.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Subastas.WebApi.Hubs;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador API para operaciones sobre la entidad Puja.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class PujasController : ControllerBase
{
    private readonly SubastaContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<PujasController> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;

    public PujasController(SubastaContext context, IEmailService emailService, ILogger<PujasController> logger, IHubContext<NotificationHub> hubContext)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Obtiene todas las pujas del sistema.
    /// </summary>
    /// <returns>Lista de pujas con información de vehículo.</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetPujas()
    {
        try
        {
            var pujas = await _context.Pujas
                .Include(p => p.Subasta)
                    .ThenInclude(s => s.Vehiculo)
                        .ThenInclude(v => v.ImagenesVehiculo)
                .Include(p => p.Usuario)
                .Select(p => new
                {
                    p.IdPuja,
                    p.IdSubasta,
                    p.IdUsuario,
                    p.Cantidad,
                    p.FechaPuja,
                    usuario = new
                    {
                        p.Usuario.IdUsuario,
                        p.Usuario.Nombre,
                        p.Usuario.Apellidos,
                        p.Usuario.Email
                    },
                    subasta = new
                    {
                        p.Subasta.IdSubasta,
                        p.Subasta.FechaInicio,
                        p.Subasta.FechaFin,
                        p.Subasta.Estado,
                        p.Subasta.PrecioInicial,
                        p.Subasta.PrecioActual,
                        vehiculo = new
                        {
                            p.Subasta.Vehiculo.IdVehiculo,
                            p.Subasta.Vehiculo.Marca,
                            p.Subasta.Vehiculo.Modelo,
                            p.Subasta.Vehiculo.Anio,
                            imagenes = p.Subasta.Vehiculo.ImagenesVehiculo.Select(img => new
                            {
                                img.IdImagen,
                                img.Nombre,
                                img.Ruta,
                                img.Activo
                            }).ToList()
                        }
                    }
                })
                .OrderByDescending(p => p.FechaPuja)
                .ToListAsync();

            return Ok(pujas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cargando pujas");
            return StatusCode(500, new { Success = false, Message = "Error al cargar pujas", Error = ex.Message });
        }
    }

    /// <summary>
    /// Obtiene todas las pujas activas incluyendo los datos completos del vehículo y sus imágenes.
    /// </summary>
    /// <returns>Lista de pujas activas con información de vehículo.</returns>
    [HttpGet("activas")]
    public async Task<ActionResult<IEnumerable<object>>> GetPujasActivas()
    {
        var ahora = DateTime.Now;

        var pujas = await _context.Pujas
            .Include(p => p.Subasta)
                .ThenInclude(s => s.Vehiculo)
                    .ThenInclude(v => v.ImagenesVehiculo)
            .Where(p => p.Subasta.FechaFin > ahora)
            .Select(p => new
            {
                puja = new
                {
                    p.IdPuja,
                    p.IdSubasta,
                    p.IdUsuario,
                    p.Cantidad,
                    p.FechaPuja
                },
                vehiculo = new
                {
                    p.Subasta.Vehiculo.IdVehiculo,
                    p.Subasta.Vehiculo.Marca,
                    p.Subasta.Vehiculo.Modelo,
                    p.Subasta.Vehiculo.Anio,
                    p.Subasta.Vehiculo.Kilometraje,
                    p.Subasta.Vehiculo.Potencia,
                    imagenes = p.Subasta.Vehiculo.ImagenesVehiculo.Select(img => new
                    {
                        img.IdImagen,
                        img.Nombre,
                        img.Ruta,
                        img.Activo
                    }).ToList()
                }
            })
            .ToListAsync();

        return Ok(pujas);
    }

    /// <summary>
    /// Obtiene las pujas realizadas por un usuario específico.
    /// </summary>
    /// <param name="idUsuario">ID del usuario cuyas pujas se desean obtener.</param>
    /// <returns>Lista de pujas del usuario con información de la subasta y el vehículo.</returns>
    [HttpGet("usuario/{idUsuario}")]
    public async Task<ActionResult<IEnumerable<object>>> GetPujasPorUsuario(int idUsuario)
    {
        try
        {
            // Load all pujas for user with related subasta and vehiculo/images
            var userPujas = await _context.Pujas
                .Include(p => p.Subasta)
                .ThenInclude(s => s.Vehiculo)
                .ThenInclude(v => v.ImagenesVehiculo)
                .Where(p => p.IdUsuario == idUsuario)
                .OrderByDescending(p => p.FechaPuja)
                .ToListAsync();

            var subastaIds = userPujas.Select(p => p.IdSubasta).Distinct().ToList();

            // Load highest puja per subasta (with user) in one query to avoid N+1
            var highestPujasList = await _context.Pujas
                .Where(p => subastaIds.Contains(p.IdSubasta))
                .Include(p => p.Usuario)
                .OrderByDescending(p => p.Cantidad)
                .ToListAsync();

            var highestBySubasta = new Dictionary<int, Puja>();
            foreach (var hp in highestPujasList)
            {
                if (!highestBySubasta.ContainsKey(hp.IdSubasta))
                {
                    highestBySubasta[hp.IdSubasta] = hp; // first encountered is the highest due to ordering
                }
            }

            var result = userPujas.Select(p =>
            {
                highestBySubasta.TryGetValue(p.IdSubasta, out var highest);
                var estaGanando = highest != null && highest.IdUsuario == p.IdUsuario;
                decimal? highestAmount = highest?.Cantidad;
                int? highestUserId = highest?.IdUsuario;
                string? highestUserName = highest?.Usuario != null ? highest.Usuario.Nombre : null;
                decimal diferencia =0;
                if (!estaGanando && highestAmount.HasValue)
                {
                    diferencia = highestAmount.Value - p.Cantidad;
                }

                return new
                {
                    p.IdPuja,
                    p.Cantidad,
                    p.FechaPuja,
                    p.IdSubasta,
                    p.IdUsuario,
                    estaGanando,
                    highest = highest != null ? new
                    {
                        userId = highestUserId,
                        userName = highestUserName,
                        amount = highestAmount
                    } : null,
                    diferencia = diferencia,
                    subasta = new
                    {
                        p.Subasta.IdSubasta,
                        p.Subasta.IdVehiculo,
                        p.Subasta.FechaInicio,
                        p.Subasta.FechaFin,
                        p.Subasta.PrecioInicial,
                        p.Subasta.PrecioActual,
                        p.Subasta.IncrementoMinimo,
                        p.Subasta.Estado,
                        vehiculo = new
                        {
                            p.Subasta.Vehiculo.IdVehiculo,
                            p.Subasta.Vehiculo.Marca,
                            p.Subasta.Vehiculo.Modelo,
                            p.Subasta.Vehiculo.Anio,
                            p.Subasta.Vehiculo.Kilometraje,
                            p.Subasta.Vehiculo.Potencia,
                            p.Subasta.Vehiculo.Color,
                            p.Subasta.Vehiculo.TipoMotor,
                            p.Subasta.Vehiculo.TipoCarroceria,
                            p.Subasta.Vehiculo.Descripcion,
                            p.Subasta.Vehiculo.Matricula,
                            p.Subasta.Vehiculo.NumeroPuertas,
                            imagenes = p.Subasta.Vehiculo.ImagenesVehiculo.Where(img => img.Activo ==1).Select(img => new
                            {
                                img.IdImagen,
                                img.Nombre,
                                img.Ruta,
                                img.Activo
                            }).ToList()
                        }
                    }
                };
            }).ToList();

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Success = false, Message = "Error al cargar pujas del usuario", Error = ex.Message, StackTrace = ex.StackTrace });
        }
    }

    /// <summary>
    /// Realiza una nueva puja.
    /// </summary>
    /// <param name="request">Datos de la puja.</param>
    /// <returns>NoContent si la puja se crea correctamente.</returns>
    [HttpPost]
    public async Task<IActionResult> PostPuja([FromBody] PujaRequest request)
    {
        var subasta = await _context.Subastas
            .Include(s => s.Vehiculo)
            .FirstOrDefaultAsync(s => s.IdSubasta == request.IdSubasta);

        var usuario = await _context.Usuarios.FindAsync(request.IdUsuario);

        if (subasta == null || usuario == null)
            return BadRequest("Subasta o usuario no válido.");

        if (usuario.Rol?.Trim().ToLower() == "admin" || usuario.Rol?.Trim().ToLower() == "administrador")
            return BadRequest(new 
            { 
                mensaje = "Los administradores no pueden realizar pujas. Los vehículos son propiedad del sistema.",
                esAdmin = true
            });

        if (usuario.Validado == 0)
            return BadRequest(new 
            { 
                mensaje = "Tu cuenta debe estar validada para poder pujar. Por favor, sube tu documento IAE y espera la validación del administrador.",
                requiereValidacion = true
            });

        if (string.IsNullOrEmpty(usuario.DocumentoIAE))
            return BadRequest(new 
            { 
                mensaje = "Debes subir tu documento IAE antes de poder pujar.",
                exigeDocumento = true
            });

        // Get current highest puja BEFORE inserting the new one
        var currentHighest = await _context.Pujas
            .Where(p => p.IdSubasta == request.IdSubasta)
            .OrderByDescending(p => p.Cantidad)
            .FirstOrDefaultAsync();

        // Determine current price
        var currentPrice = subasta.PrecioActual ?? subasta.PrecioInicial;

        // Enforce bid rules
        if (subasta.PrecioActual == null)
        {
            // No previous bids: bid must be >= PrecioInicial
            if (request.Cantidad < subasta.PrecioInicial)
            {
                return BadRequest(new { mensaje = $"La puja debe ser al menos el precio inicial: {subasta.PrecioInicial:C}.", required = subasta.PrecioInicial });
            }
        }
        else
        {
            // There is a current price: bid must be strictly greater than current and at least current + incremento
            if (request.Cantidad <= subasta.PrecioActual)
            {
                return BadRequest(new { mensaje = "La puja debe ser mayor que el precio actual.", precioActual = subasta.PrecioActual });
            }
            if (subasta.IncrementoMinimo >0 && request.Cantidad < subasta.PrecioActual + subasta.IncrementoMinimo)
            {
                var minimo = subasta.PrecioActual + subasta.IncrementoMinimo;
                return BadRequest(new { mensaje = $"La puja debe ser al menos {minimo:C} (precio actual + incremento mínimo).", required = minimo });
            }
        }

        // Create puja
        var puja = new Puja
        {
            IdSubasta = request.IdSubasta,
            IdUsuario = request.IdUsuario,
            Cantidad = request.Cantidad,
            FechaPuja = request.FechaPuja == default ? DateTime.UtcNow : request.FechaPuja
        };

        _context.Pujas.Add(puja);

        // Update subasta.PrecioActual if this bid is higher than current
        currentPrice = subasta.PrecioActual ?? subasta.PrecioInicial;
        if (puja.Cantidad > currentPrice)
        {
            subasta.PrecioActual = puja.Cantidad;
            _logger.LogInformation("Updating Subasta {SubastaId} PrecioActual to {Precio}", subasta.IdSubasta, subasta.PrecioActual);
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Nueva puja creada: IdPuja={IdPuja}, Subasta={SubastaId}, Usuario={UsuarioId}, Cantidad={Cantidad}", puja.IdPuja, puja.IdSubasta, puja.IdUsuario, puja.Cantidad);

        try
        {
            // Confirmation notif for the bidder
            var mensajeConfirm = $"Tu puja de {puja.Cantidad:C} en la subasta #{subasta.IdSubasta} {(subasta.Vehiculo != null ? $"({subasta.Vehiculo.Marca} {subasta.Vehiculo.Modelo})" : string.Empty)} se ha registrado correctamente.";
            var confirmNotif = new Notificacion
            {
                IdUsuario = usuario.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                Mensaje = mensajeConfirm,
                FechaEnvio = DateTime.UtcNow,
                Leida =0
            };

            _context.Notificaciones.Add(confirmNotif);
            _logger.LogInformation("Prepared confirmation notification for user {UserId}", usuario.IdUsuario);

            // Notify ALL distinct previous bidders (except the current bidder)
            var previousBidders = await _context.Pujas
                .Where(p => p.IdSubasta == subasta.IdSubasta && p.IdUsuario != request.IdUsuario)
                .Select(p => p.IdUsuario)
                .Distinct()
                .ToListAsync();

            _logger.LogInformation("Found {Count} previous distinct bidders for subasta {SubastaId}", previousBidders.Count, subasta.IdSubasta);

            var outbidNotifs = new List<Notificacion>();
            foreach (var prevUserId in previousBidders)
            {
                var prev = await _context.Usuarios.FindAsync(prevUserId);
                if (prev == null)
                {
                    _logger.LogWarning("Previous bidder id {PrevUserId} not found in Usuarios table", prevUserId);
                    continue;
                }

                var mensaje = $"Has sido superado en la subasta #{subasta.IdSubasta} {(subasta.Vehiculo != null ? $"({subasta.Vehiculo.Marca} {subasta.Vehiculo.Modelo})" : string.Empty)} por {usuario.Nombre} {usuario.Apellidos} con {request.Cantidad:C}.";

                var outbidNotif = new Notificacion
                {
                    IdUsuario = prev.IdUsuario,
                    IdSubasta = subasta.IdSubasta,
                    Mensaje = mensaje,
                    FechaEnvio = DateTime.UtcNow,
                    Leida =0
                };

                _context.Notificaciones.Add(outbidNotif);
                outbidNotifs.Add(outbidNotif);
                _logger.LogInformation("Added outbid notification for previous user {PrevUserId} on subasta {SubastaId}", prev.IdUsuario, subasta.IdSubasta);
            }

            // Admin notification for audit
            var adminNot = new NotificacionAdmin
            {
                Titulo = "Nueva puja",
                Mensaje = $"Usuario {usuario.Email} realizó una nueva puja de {request.Cantidad:C} en la subasta #{subasta.IdSubasta}.",
                Tipo = "puja",
                IdUsuario = usuario.IdUsuario,
                Leida =0,
                FechaCreacion = DateTime.UtcNow
            };

            _context.NotificacionesAdmin.Add(adminNot);

            // Persist all notifications
            _logger.LogInformation("Saving notifications to database (count pending: {Count})", _context.ChangeTracker.Entries().Count(e => e.Entity is Notificacion || e.Entity is NotificacionAdmin));
            await _context.SaveChangesAsync();
            _logger.LogInformation("Notifications saved to DB for puja {PujaId}", puja.IdPuja);

            // Send real-time notifications after save
            // Confirmation to bidder
            await _hubContext.Clients.Group($"user_{usuario.IdUsuario}").SendAsync("ReceiveNotification", new
            {
                confirmNotif.IdNotificacion,
                Tipo = "usuario",
                Titulo = (string?)null,
                Mensaje = confirmNotif.Mensaje,
                Fecha = confirmNotif.FechaEnvio,
                Leida = confirmNotif.Leida == 1,
                UsuarioId = confirmNotif.IdUsuario,
                SubastaId = confirmNotif.IdSubasta
            });

            // Outbid to previous bidders
            foreach (var outbidNotif in outbidNotifs)
            {
                await _hubContext.Clients.Group($"user_{outbidNotif.IdUsuario}").SendAsync("ReceiveNotification", new
                {
                    outbidNotif.IdNotificacion,
                    Tipo = "usuario",
                    Titulo = (string?)null,
                    Mensaje = outbidNotif.Mensaje,
                    Fecha = outbidNotif.FechaEnvio,
                    Leida = outbidNotif.Leida == 1,
                    UsuarioId = outbidNotif.IdUsuario,
                    SubastaId = outbidNotif.IdSubasta
                });

                var prev = await _context.Usuarios.FindAsync(outbidNotif.IdUsuario);
                if (prev != null)
                {
                    try
                    {
                        await _emailService.EnviarEmailUsuarioAsync(prev.Email, prev.Nombre, "Has sido superado en una puja", outbidNotif.Mensaje);
                        _logger.LogInformation("Email attempt to previous bidder {Email} completed", prev.Email);
                    }
                    catch (Exception mailEx)
                    {
                        _logger.LogWarning(mailEx, "Fallo al enviar email a previous bidder {Email}", prev.Email);
                    }
                }
            }

            // Admin notification
            await _hubContext.Clients.Group("admins").SendAsync("ReceiveNotification", new
            {
                adminNot.IdNotificacion,
                adminNot.Titulo,
                adminNot.Mensaje,
                adminNot.Tipo,
                adminNot.IdUsuario,
                adminNot.Leida,
                adminNot.FechaCreacion
            });

            // confirmation email to bidder
            try
            {
                await _emailService.EnviarEmailUsuarioAsync(usuario.Email, usuario.Nombre, "Confirmación de puja", mensajeConfirm);
                _logger.LogInformation("Email attempt to bidder {Email} completed", usuario.Email);
            }
            catch (Exception mailEx)
            {
                _logger.LogWarning(mailEx, "Fallo al enviar email de confirmación a {Email}", usuario.Email);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creando o guardando notificaciones tras la puja. Aborting and returning500.");
            return StatusCode(500, new { Success = false, Message = "Error al crear notificaciones" , Detail = ex.Message});
        }

        return NoContent();
    }

    /// <summary>
    /// Finaliza una subasta manualmente: calcula ganador, notifica ganador, perdedores y admins.
    /// </summary>
    [HttpPost("finalizar/{id}")]
    public async Task<IActionResult> FinalizarSubasta(int id)
    {
        var subasta = await _context.Subastas
            .Include(s => s.Vehiculo)
            .FirstOrDefaultAsync(s => s.IdSubasta == id);

        if (subasta == null) return NotFound();
        if (subasta.Estado == "finalizada") return BadRequest("Subasta ya finalizada");

        // Get all bids for the auction
        var pujas = await _context.Pujas
            .Where(p => p.IdSubasta == id)
            .OrderByDescending(p => p.Cantidad)
            .ToListAsync();

        if (!pujas.Any())
        {
            subasta.Estado = "finalizada";
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Subasta finalizada sin pujas" });
        }

        var winner = pujas.First();
        var losers = pujas.Where(p => p.IdUsuario != winner.IdUsuario).Select(p => p.IdUsuario).Distinct().ToList();

        // Update subasta final price and state
        subasta.PrecioActual = winner.Cantidad;
        subasta.Estado = "finalizada";

        // Notify winner
        var winnerUser = await _context.Usuarios.FindAsync(winner.IdUsuario);
        if (winnerUser != null)
        {
            var msg = $"¡Has ganado la subasta #{subasta.IdSubasta} con {winner.Cantidad:C}! En 30 días máximo, el administrador te contactará para ultimar los detalles de la venta.";
            _context.Notificaciones.Add(new Notificacion
            {
                IdUsuario = winnerUser.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                Mensaje = msg,
                FechaEnvio = DateTime.UtcNow,
                Leida =0
            });

            try { await _emailService.EnviarEmailUsuarioAsync(winnerUser.Email, winnerUser.Nombre, "Has ganado la subasta", msg); } catch { }
        }

        // Notify losers
        foreach (var loserId in losers)
        {
            var loser = await _context.Usuarios.FindAsync(loserId);
            if (loser == null) continue;
            var msg = $"No has ganado la subasta #{subasta.IdSubasta}. Puja ganadora: {winner.Cantidad:C}.";
            _context.Notificaciones.Add(new Notificacion
            {
                IdUsuario = loser.IdUsuario,
                IdSubasta = subasta.IdSubasta,
                Mensaje = msg,
                FechaEnvio = DateTime.UtcNow,
                Leida =0
            });

            try { await _emailService.EnviarEmailUsuarioAsync(loser.Email, loser.Nombre, "Subasta finalizada - resultado", msg); } catch { }
        }

        // Admin notification
        _context.NotificacionesAdmin.Add(new NotificacionAdmin
        {
            Titulo = "Subasta finalizada",
            Mensaje = $"Subasta #{subasta.IdSubasta} finalizada. Ganador: {winnerUser?.Email ?? winner.IdUsuario.ToString()} - {winner.Cantidad:C}",
            Tipo = "finalizacion",
            IdUsuario = null, // Set to null for all admins
            Leida =0,
            FechaCreacion = DateTime.UtcNow
        });

        await _context.SaveChangesAsync();

        return Ok(new { Message = "Subasta finalizada y notificaciones enviadas" });
    }

    /// <summary>
    /// Obtiene los detalles de una subasta ganada por el usuario actual.
    /// </summary>
    /// <param name="id">ID de la subasta.</param>
    /// <returns>Detalles de la subasta ganada.</returns>
    [HttpGet("subasta-ganada/{id}")]
    [Authorize]
    public async Task<IActionResult> GetSubastaGanada(int id)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var subasta = await _context.Subastas
            .Include(s => s.Vehiculo)
            .ThenInclude(v => v.ImagenesVehiculo)
            .FirstOrDefaultAsync(s => s.IdSubasta == id && s.Estado == "finalizada");

        if (subasta == null)
        {
            return NotFound("Subasta no encontrada");
        }

        // Check if user is the winner
        var winnerPuja = await _context.Pujas
            .Where(p => p.IdSubasta == id)
            .OrderByDescending(p => p.Cantidad)
            .FirstOrDefaultAsync();

        if (winnerPuja == null || winnerPuja.IdUsuario != userId)
        {
            return Forbid("No eres el ganador de esta subasta");
        }

        var result = new
        {
            subasta.IdSubasta,
            subasta.FechaInicio,
            subasta.FechaFin,
            subasta.PrecioInicial,
            subasta.PrecioActual,
            subasta.IncrementoMinimo,
            subasta.Estado,
            vehiculo = new
            {
                subasta.Vehiculo.IdVehiculo,
                subasta.Vehiculo.Marca,
                subasta.Vehiculo.Modelo,
                subasta.Vehiculo.Anio,
                subasta.Vehiculo.Kilometraje,
                subasta.Vehiculo.Potencia,
                subasta.Vehiculo.Color,
                subasta.Vehiculo.TipoMotor,
                subasta.Vehiculo.TipoCarroceria,
                subasta.Vehiculo.Descripcion,
                subasta.Vehiculo.Matricula,
                subasta.Vehiculo.NumeroPuertas,
                imagenes = subasta.Vehiculo.ImagenesVehiculo.Where(img => img.Activo == 1).Select(img => new
                {
                    img.IdImagen,
                    img.Nombre,
                    img.Ruta,
                    img.Activo
                }).ToList()
            },
            mensajeAdicional = "En 30 días máximo, el administrador te contactará para ultimar los detalles de la venta."
        };

        return Ok(result);
    }
}

/// <summary>
/// Modelo de datos para crear una puja.
/// </summary>
public class PujaRequest
{
    public int IdSubasta { get; set; }
    public int IdUsuario { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime FechaPuja { get; set; }
}
