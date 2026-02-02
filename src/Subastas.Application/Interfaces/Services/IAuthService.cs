namespace Subastas.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de autenticación.
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Genera un token JWT para un usuario.
    /// </summary>
    string GenerateJwtToken(int userId, string email, string rol);

    /// <summary>
    /// Valida un token JWT.
    /// </summary>
    bool ValidateToken(string token);

    /// <summary>
    /// Extrae el ID de usuario de un token.
    /// </summary>
    int? GetUserIdFromToken(string token);
}
