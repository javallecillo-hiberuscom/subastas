using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;
using Subastas.Application.Interfaces.Services;

namespace Subastas.WebApi.Controllers;

/// <summary>
/// Controlador para gestión de usuarios y autenticación.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _usuarioService;
    private readonly ILogger<UsuariosController> _logger;

    public UsuariosController(
        IUsuarioService usuarioService,
        ILogger<UsuariosController> logger)
    {
        _usuarioService = usuarioService;
        _logger = logger;
    }

    /// <summary>
    /// Obtiene todos los usuarios del sistema.
    /// </summary>
    [HttpGet]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ApiResponse<IEnumerable<UsuarioResponse>>>> GetUsuarios()
    {
        var usuarios = await _usuarioService.ObtenerTodosAsync();
        return Ok(ApiResponse<IEnumerable<UsuarioResponse>>.SuccessResult(
            usuarios, "Usuarios obtenidos correctamente"));
    }

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> GetUsuario(int id)
    {
        var usuario = await _usuarioService.ObtenerPorIdAsync(id);
        if (usuario == null)
        {
            return NotFound(ApiResponse<UsuarioResponse>.ErrorResult("Usuario no encontrado"));
        }
        return Ok(ApiResponse<UsuarioResponse>.SuccessResult(usuario));
    }

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    [HttpPost("registro")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> Registro([FromBody] RegistroUsuarioRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<UsuarioResponse>.ErrorResult(
                "Datos de registro inválidos"));
        }

        var response = await _usuarioService.RegistrarAsync(request);
        return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
            response, "Usuario registrado correctamente. Pendiente de validación."));
    }

    /// <summary>
    /// Inicia sesión y devuelve un token JWT.
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LoginResponse>>> Login([FromBody] LoginRequest request)
    {
        if (request == null)
        {
            return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                "Datos de login inválidos"));
        }

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<LoginResponse>.ErrorResult(
                "Datos de login inválidos", errors));
        }

        var response = await _usuarioService.LoginAsync(request);
        return Ok(ApiResponse<LoginResponse>.SuccessResult(response, "Login exitoso"));
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
        if (request == null)
        {
            return BadRequest(ApiResponse<UsuarioResponse>.ErrorResult(
                "Datos de actualización inválidos"));
        }

        var response = await _usuarioService.ActualizarPerfilAsync(id, request);
        return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
            response, "Perfil actualizado correctamente"));
    }

    /// <summary>
    /// Valida un usuario (solo administradores).
    /// </summary>
    [HttpPut("{id}/validar")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ApiResponse<UsuarioResponse>>> ValidarUsuario(int id)
    {
        var response = await _usuarioService.ValidarUsuarioAsync(id);
        return Ok(ApiResponse<UsuarioResponse>.SuccessResult(
            response, "Usuario validado correctamente"));
    }

    /// <summary>
    /// Elimina un usuario (solo administradores).
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminPolicy")]
    public async Task<ActionResult<ApiResponse<object>>> EliminarUsuario(int id)
    {
        await _usuarioService.EliminarUsuarioAsync(id);
        return Ok(ApiResponse<object>.SuccessResult(
            new { }, "Usuario eliminado correctamente"));
    }
}
