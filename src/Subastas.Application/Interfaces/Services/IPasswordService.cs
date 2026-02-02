namespace Subastas.Application.Interfaces.Services;

/// <summary>
/// Interfaz para el servicio de gestión de contraseñas.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Hashea una contraseña usando BCrypt.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Verifica si una contraseña coincide con su hash.
    /// </summary>
    bool VerifyPassword(string password, string hashedPassword);
}
