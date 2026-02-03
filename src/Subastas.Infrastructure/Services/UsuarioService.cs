using Microsoft.Extensions.Logging;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Application.Interfaces.Services;
using Subastas.Domain.Entities;
using Subastas.Domain.Exceptions;

namespace Subastas.Infrastructure.Services;

/// <summary>
/// Servicio para gestión de usuarios y autenticación.
/// </summary>
public class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAuthService _authService;
    private readonly INotificacionAdminService _notificacionAdminService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuarioService> _logger;

    public UsuarioService(
        IUsuarioRepository usuarioRepository,
        IPasswordService passwordService,
        IAuthService authService,
        INotificacionAdminService notificacionAdminService,
        IEmailService emailService,
        ILogger<UsuarioService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _authService = authService;
        _notificacionAdminService = notificacionAdminService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<IEnumerable<UsuarioResponse>> ObtenerTodosAsync()
    {
        var usuarios = await _usuarioRepository.GetAllAsync();
        return usuarios.Select(MapearAResponse);
    }

    public async Task<UsuarioResponse?> ObtenerPorIdAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException("Usuario", id);
        }

        return MapearAResponse(usuario);
    }

    public async Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request)
    {
        _logger.LogInformation("=== REGISTRO REQUEST ===");
        _logger.LogInformation($"Nombre: {request.Nombre}");
        _logger.LogInformation($"Apellidos: {request.Apellidos}");
        _logger.LogInformation($"Email: {request.Email}");
        _logger.LogInformation($"IdEmpresa: {request.IdEmpresa}");
        _logger.LogInformation($"Telefono: {request.Telefono}");
        _logger.LogInformation($"Direccion: {request.Direccion}");

        // Validar si el email ya existe
        if (await _usuarioRepository.EmailExistsAsync(request.Email))
        {
            throw new DuplicateEntityException("El email ya está registrado");
        }

        // Crear nuevo usuario
        var usuario = new Usuario
        {
            Nombre = request.Nombre,
            Apellidos = request.Apellidos,
            Email = request.Email,
            Password = _passwordService.HashPassword(request.Password),
            Rol = "registrado",
            Activo = 1,
            Validado = 0,
            IdEmpresa = request.IdEmpresa,
            Telefono = request.Telefono,
            Direccion = request.Direccion
        };

        _logger.LogWarning(">>> PUNTO 1: Guardando usuario en BD...");
        await _usuarioRepository.AddAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
        _logger.LogWarning(">>> PUNTO 2: Usuario ID={IdUsuario} guardado", usuario.IdUsuario);

        try
        {
            _logger.LogWarning(">>> PUNTO 3: Llamando a notificación ID={IdUsuario}", usuario.IdUsuario);
            await _notificacionAdminService.CrearNotificacionRegistroAsync(
                usuario.IdUsuario,
                $"{usuario.Nombre} {usuario.Apellidos}".Trim(),
                usuario.Email);
            _logger.LogWarning(">>> PUNTO 4: Notificación EXITOSA ID={IdUsuario}", usuario.IdUsuario);
        }
        catch (Exception notifEx)
        {
            _logger.LogError(notifEx, "=== ERROR FATAL al crear notificación: {Message} ===", notifEx.Message);
            _logger.LogError("Tipo de excepción: {Type}", notifEx.GetType().FullName);
            _logger.LogError("StackTrace completo: {StackTrace}", notifEx.StackTrace);
        }

        return MapearAResponse(usuario);
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        _logger.LogInformation($"=== LOGIN REQUEST para {request.Email} ===");

        var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
        if (usuario == null)
        {
            throw new InvalidCredentialsException();
        }

        // Verificar contraseña
        if (!_passwordService.VerifyPassword(request.Password, usuario.Password))
        {
            throw new InvalidCredentialsException();
        }

        // Verificar que el usuario esté activo
        if (usuario.Activo != 1)
        {
            throw new BusinessRuleException("Usuario inactivo o eliminado");
        }

        // Generar token JWT
        var token = _authService.GenerateJwtToken(
            usuario.IdUsuario,
            usuario.Email,
            usuario.Rol);

        return new LoginResponse
        {
            Token = token,
            IdUsuario = usuario.IdUsuario,
            Email = usuario.Email,
            NombreCompleto = $"{usuario.Nombre} {usuario.Apellidos}".Trim(),
            Rol = usuario.Rol,
            Validado = usuario.Validado == 1
        };
    }

    public async Task<UsuarioResponse> ActualizarPerfilAsync(int id, ActualizarPerfilRequest request)
    {
        if (id != request.IdUsuario)
        {
            throw new BusinessRuleException("ID de usuario no coincide");
        }

        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException("Usuario", id);
        }

        // Actualizar campos
        usuario.Nombre = request.Nombre;
        usuario.Apellidos = request.Apellidos;
        usuario.Email = request.Email;
        usuario.Rol = request.Rol;
        usuario.Activo = request.Activo;
        usuario.Telefono = request.Telefono;
        usuario.Direccion = request.Direccion;

        if (request.IdEmpresa.HasValue)
        {
            usuario.IdEmpresa = request.IdEmpresa.Value;
        }

        // Actualizar contraseña solo si se proporciona
        if (!string.IsNullOrEmpty(request.Password))
        {
            usuario.Password = _passwordService.HashPassword(request.Password);
        }

        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        return MapearAResponse(usuario);
    }

    public async Task<UsuarioResponse> ValidarUsuarioAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException("Usuario", id);
        }

        // Validar usuario
        usuario.Validado = 1;

        await _usuarioRepository.UpdateAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();

        // Enviar email de notificación al usuario
        await _emailService.EnviarEmailUsuarioAsync(
            usuario.Email,
            $"{usuario.Nombre} {usuario.Apellidos}".Trim(),
            "Cuenta validada",
            $"¡Hola {usuario.Nombre}!<br><br>Tu cuenta ha sido validada por un administrador. Ahora puedes participar en las subastas y realizar pujas.<br><br>¡Bienvenido!");

        return MapearAResponse(usuario);
    }

    public async Task EliminarUsuarioAsync(int id)
    {
        var usuario = await _usuarioRepository.GetByIdAsync(id);
        if (usuario == null)
        {
            throw new EntityNotFoundException("Usuario", id);
        }

        // Verificar que no sea el último administrador
        if (usuario.Rol?.Trim().ToLower() == "admin")
        {
            var adminCount = await _usuarioRepository.CountAdminsAsync();
            if (adminCount <= 1)
            {
                throw new BusinessRuleException("No se puede eliminar el último administrador del sistema");
            }
        }

        await _usuarioRepository.DeleteAsync(usuario);
        await _usuarioRepository.SaveChangesAsync();
    }

    private static UsuarioResponse MapearAResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            IdUsuario = usuario.IdUsuario,
            Nombre = usuario.Nombre,
            Apellidos = usuario.Apellidos,
            Email = usuario.Email,
            Rol = usuario.Rol,
            Activo = usuario.Activo == 1,
            Validado = usuario.Validado == 1,
            Telefono = usuario.Telefono,
            Direccion = usuario.Direccion,
            IdEmpresa = usuario.IdEmpresa,
            FotoPerfilBase64 = usuario.FotoPerfilBase64,
            DocumentoIAE = usuario.DocumentoIAE
        };
    }
}
