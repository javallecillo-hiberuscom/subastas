using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Application.Interfaces.Services;
using Subastas.Domain.Entities;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador para gestión de usuarios y autenticación.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IPasswordService _passwordService;
    private readonly IAuthService _authService;
    private readonly INotificacionAdminService _notificacionAdminService;
    private readonly IEmailService _emailService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        IUsuarioRepository usuarioRepository,
        IPasswordService passwordService,
        IAuthService authService,
        INotificacionAdminService notificacionAdminService,
        IEmailService emailService,
        ILogger<UsuariosController> logger)
    {
        _usuarioRepository = usuarioRepository;
        _passwordService = passwordService;
        _authService = authService;
        _notificacionAdminService = notificacionAdminService;
        _emailService = emailService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResponse>>>> GetUsuarios()
    {
        try
        {
            var usuarios = await _usuarioRepository.GetAllAsync();
            var response = usuarios.Select(u => new UsuarioResponse
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Apellidos = u.Apellidos,
                Email = u.Email,
                Rol = u.Rol,
                Activo = u.Activo == 1,
                Validado = u.Validado == 1,
                Telefono = u.Telefono,
                Direccion = u.Direccion,
                IdEmpresa = u.IdEmpresa,
                FotoPerfilBase64 = u.FotoPerfilBase64,
                DocumentoIAE = u.DocumentoIAE
            });

            return Ok(ApiResponse<IEnumerable<UsuarioResponse>>.SuccessResult(
                response, "Usuarios obtenidos correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuarios");
            return StatusCode(500, ApiResponse<IEnumerable<UsuarioResponse>>.ErrorResult(
                "Error al obtener usuarios", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> GetUsuario(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioResponse>.ErrorResult("Usuario no encontrado"));
            }

            var response = new UsuarioResponse
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

            return Ok(ApiResponse<UsuarioResponse>.SuccessResult(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener usuario {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioResponse>.ErrorResult(
                "Error al obtener usuario", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> Registro([FromBody] RegistroUsuarioRequest request)
    {
        try
        {
            // Validar si el email ya existe
            if (await _usuarioRepository.EmailExistsAsync(request.Email))
            {
                return BadRequest(ApiResponse<UsuarioResponse>.ErrorResult(
                    "El email ya está registrado"));
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
                IdEmpresa = request.IdEmpresa ?? 1, // Valor por defecto
                Telefono = request.Telefono,
                Direccion = request.Direccion
            };

            await _usuarioRepository.AddAsync(usuario);
            await _usuarioRepository.SaveChangesAsync();

            // Crear notificación administrativa
            await _notificacionAdminService.CrearNotificacionRegistroAsync(
                usuario.IdUsuario,
                $"{usuario.Nombre} {usuario.Apellidos}".Trim(),
                usuario.Email);

            var response = new UsuarioResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Activo = true,
                Validado = false
            };

            return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
                response, "Usuario registrado correctamente. Pendiente de validación."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al registrar usuario");
            return StatusCode(500, ApiResponse<UsuarioResponse>.ErrorResult(
                "Error al registrar usuario", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            Console.WriteLine($"=== LOGIN REQUEST ===");
            Console.WriteLine($"Email recibido: {request?.Email ?? "NULL"}");
            Console.WriteLine($"Password recibido: {request?.Password ?? "NULL"}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                Console.WriteLine($"Errores de validación: {string.Join(", ", errors)}");
                return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                    "Datos de login inválidos", errors));
            }

            var usuario = await _usuarioRepository.GetByEmailAsync(request.Email);
            Console.WriteLine($"Usuario encontrado: {usuario != null}");
            
            if (usuario == null)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult(
                    "Credenciales inválidas"));
            }

            // Verificar contraseña
            bool passwordValido = _passwordService.VerifyPassword(request.Password, usuario.Password);
            Console.WriteLine($"Password válido: {passwordValido}");
            
            if (!passwordValido)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult(
                    "Credenciales inválidas"));
            }

            // Verificar que el usuario esté activo
            if (usuario.Activo != 1)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult(
                    "Usuario inactivo o eliminado"));
            }

            // Solo los usuarios normales necesitan estar validados, los admins no
            if (usuario.Rol != "Admin" && usuario.Validado != 1)
            {
                return Unauthorized(ApiResponse<LoginResponse>.ErrorResult(
                    "Usuario pendiente de validación por el administrador"));
            }

            // Generar token JWT
            var token = _authService.GenerateJwtToken(
                usuario.IdUsuario,
                usuario.Email,
                usuario.Rol);

            var response = new LoginResponse
            {
                Token = token,
                IdUsuario = usuario.IdUsuario,
                Email = usuario.Email,
                NombreCompleto = $"{usuario.Nombre} {usuario.Apellidos}".Trim(),
                Rol = usuario.Rol
            };

            return Ok(ApiResponse<LoginResponse>.SuccessResult(
                response, "Login exitoso"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en login");
            return StatusCode(500, ApiResponse<LoginResponse>.ErrorResult(
                "Error al procesar login", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Actualiza el perfil de un usuario.
    /// </summary>
    [HttpPut("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> ActualizarPerfil(
        int id, 
        [FromBody] ActualizarPerfilRequest request)
    {
        try
        {
            if (id != request.IdUsuario)
            {
                return BadRequest(ApiResponse<UsuarioResponse>.ErrorResult(
                    "ID de usuario no coincide"));
            }

            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioResponse>.ErrorResult(
                    "Usuario no encontrado"));
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

            var response = new UsuarioResponse
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
                IdEmpresa = usuario.IdEmpresa
            };

            return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
                response, "Perfil actualizado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar perfil del usuario {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioResponse>.ErrorResult(
                "Error al actualizar perfil", new List<string> { ex.Message }));
        }
    }

    /// <summary>
    /// Valida un usuario (solo administradores).
    /// </summary>
    [HttpPut("{id}/validar")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> ValidarUsuario(int id)
    {
        try
        {
            var usuario = await _usuarioRepository.GetByIdAsync(id);
            if (usuario == null)
            {
                return NotFound(ApiResponse<UsuarioResponse>.ErrorResult(
                    "Usuario no encontrado"));
            }

            // Validar que el usuario tenga documento IAE subido
            if (string.IsNullOrEmpty(usuario.DocumentoIAE))
            {
                return BadRequest(ApiResponse<UsuarioResponse>.ErrorResult(
                    "El usuario debe subir su documento IAE antes de ser validado"));
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

            var response = new UsuarioResponse
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellidos = usuario.Apellidos,
                Email = usuario.Email,
                Rol = usuario.Rol,
                Activo = usuario.Activo == 1,
                Validado = true,
                Telefono = usuario.Telefono,
                Direccion = usuario.Direccion,
                IdEmpresa = usuario.IdEmpresa,
                DocumentoIAE = usuario.DocumentoIAE
            };

            return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
                response, "Usuario validado correctamente"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al validar usuario {Id}", id);
            return StatusCode(500, ApiResponse<UsuarioResponse>.ErrorResult(
                "Error al validar usuario", new List<string> { ex.Message }));
        }
    }
}
