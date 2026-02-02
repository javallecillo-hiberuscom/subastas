using System.ComponentModel.DataAnnotations;

namespace Subastas.Application.DTOs.Requests;

/// <summary>
/// DTO para la solicitud de inicio de sesión.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida.")]
    public string Password { get; set; } = null!;
}
