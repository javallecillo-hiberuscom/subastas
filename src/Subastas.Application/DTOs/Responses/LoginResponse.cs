namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta para login exitoso.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Token JWT para autenticación.
    /// </summary>
    public string Token { get; set; } = null!;

    /// <summary>
    /// ID del usuario autenticado.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Email del usuario.
    /// </summary>
    public string Email { get; set; } = null!;

    /// <summary>
    /// Nombre completo del usuario.
    /// </summary>
    public string NombreCompleto { get; set; } = null!;

    /// <summary>
    /// Rol del usuario en el sistema.
    /// </summary>
    public string Rol { get; set; } = null!;
}
