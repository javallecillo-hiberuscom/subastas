using Subastas.Application.Interfaces.Services;

namespace Subastas.Infrastructure.Services;

/// <summary>
/// Servicio para gestión segura de contraseñas usando BCrypt.
/// </summary>
public class PasswordService : IPasswordService
{
    /// <summary>
    /// Hashea una contraseña usando BCrypt con factor de trabajo predeterminado.
    /// </summary>
    /// <param name="password">Contraseña en texto plano</param>
    /// <returns>Hash de la contraseña</returns>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>
    /// Verifica si una contraseña coincide con su hash almacenado.
    /// </summary>
    /// <param name="password">Contraseña en texto plano</param>
    /// <param name="hashedPassword">Hash almacenado</param>
    /// <returns>True si coinciden, false si no</returns>
    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
