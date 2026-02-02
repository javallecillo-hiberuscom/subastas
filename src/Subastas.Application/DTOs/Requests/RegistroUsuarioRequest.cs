using System.ComponentModel.DataAnnotations;

namespace Subastas.Application.DTOs.Requests;

/// <summary>
/// DTO para la solicitud de registro de usuario.
/// </summary>
public class RegistroUsuarioRequest
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    [StringLength(100, ErrorMessage = "Los apellidos no pueden exceder 100 caracteres.")]
    public string? Apellidos { get; set; }

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres.")]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required(ErrorMessage = "La contraseña es requerida.")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
    [StringLength(255, ErrorMessage = "La contraseña no puede exceder 255 caracteres.")]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Teléfono de contacto.
    /// </summary>
    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    [StringLength(255, ErrorMessage = "La dirección no puede exceder 255 caracteres.")]
    public string? Direccion { get; set; }

    /// <summary>
    /// ID de la empresa (si aplica).
    /// </summary>
    public int? IdEmpresa { get; set; }
}
