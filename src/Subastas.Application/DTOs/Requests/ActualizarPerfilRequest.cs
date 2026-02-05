using System.ComponentModel.DataAnnotations;

namespace Subastas.Application.DTOs.Requests;

/// <summary>
/// DTO para actualización de perfil de usuario.
/// </summary>
public class ActualizarPerfilRequest
{
    /// <summary>
    /// ID del usuario a actualizar.
    /// </summary>
    [Required]
    public int IdUsuario { get; set; }

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required(ErrorMessage = "El email es requerido.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    [StringLength(100)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required(ErrorMessage = "El nombre es requerido.")]
    [StringLength(100)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    [StringLength(100)]
    public string? Apellidos { get; set; }

    /// <summary>
    /// Rol del usuario.
    /// </summary>
    [Required]
    [StringLength(255)]
    public string Rol { get; set; } = null!;

    /// <summary>
    /// Estado activo del usuario.
    /// Use boolean to match frontend (true = activo).
    /// </summary>
    public bool Activo { get; set; }

    /// <summary>
    /// Teléfono de contacto.
    /// </summary>
    [StringLength(20)]
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    [StringLength(255)]
    public string? Direccion { get; set; }

    /// <summary>
    /// ID de la empresa asociada.
    /// </summary>
    public int? IdEmpresa { get; set; }

    /// <summary>
    /// Nueva contraseña (opcional, solo si se cambia).
    /// </summary>
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos6 caracteres.")]
    [StringLength(255)]
    public string? Password { get; set; }
}
