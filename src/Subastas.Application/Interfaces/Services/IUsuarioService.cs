using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;

namespace Subastas.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de gestión de usuarios.
/// </summary>
public interface IUsuarioService
{
    /// <summary>
    /// Obtiene todos los usuarios del sistema.
    /// </summary>
    Task<IEnumerable<UsuarioResponse>> ObtenerTodosAsync();

    /// <summary>
    /// Obtiene un usuario por su ID.
    /// </summary>
    Task<UsuarioResponse?> ObtenerPorIdAsync(int id);

    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    Task<UsuarioResponse> RegistrarAsync(RegistroUsuarioRequest request);

    /// <summary>
    /// Inicia sesión y devuelve un token JWT.
    /// </summary>
    Task<LoginResponse> LoginAsync(LoginRequest request);

    /// <summary>
    /// Actualiza el perfil de un usuario.
    /// </summary>
    Task<UsuarioResponse> ActualizarPerfilAsync(int id, ActualizarPerfilRequest request);

    /// <summary>
    /// Valida un usuario (marca como validado).
    /// </summary>
    Task<UsuarioResponse> ValidarUsuarioAsync(int id);

    /// <summary>
    /// Elimina un usuario del sistema.
    /// </summary>
    Task EliminarUsuarioAsync(int id);
}
