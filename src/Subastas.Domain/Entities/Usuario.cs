using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa un usuario del sistema de subastas.
/// </summary>
[Table("Usuario")]
[Index(nameof(Email), Name = "UQ__Usuario__AB6E616411E2D183", IsUnique = true)]
public class Usuario
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    [Key]
    public int IdUsuario { get; set; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [StringLength(100)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// Apellidos del usuario.
    /// </summary>
    [StringLength(100)]
    [Unicode(false)]
    public string? Apellidos { get; set; }

    /// <summary>
    /// Correo electrónico del usuario (único).
    /// </summary>
    [StringLength(100)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    /// <summary>
    /// Contraseña del usuario (hash).
    /// </summary>
    [StringLength(255)]
    [Unicode(false)]
    public string Password { get; set; } = null!;

    /// <summary>
    /// Rol del usuario en el sistema (Admin, Usuario, etc.).
    /// </summary>
    [StringLength(255)]
    public string Rol { get; set; } = null!;

    /// <summary>
    /// Indica si el usuario está activo.
    /// </summary>
    public byte Activo { get; set; }

    /// <summary>
    /// Indica si el usuario ha sido validado.
    /// </summary>
    public byte Validado { get; set; }

    /// <summary>
    /// Identificador de la empresa a la que pertenece el usuario.
    /// </summary>
    public int? IdEmpresa { get; set; }

    /// <summary>
    /// Teléfono de contacto del usuario.
    /// </summary>
    [StringLength(20)]
    [Unicode(false)]
    public string? Telefono { get; set; }

    /// <summary>
    /// Dirección del usuario.
    /// </summary>
    [StringLength(255)]
    [Unicode(false)]
    public string? Direccion { get; set; }

    /// <summary>
    /// Foto de perfil del usuario codificada en Base64.
    /// </summary>
    [Column(TypeName = "varchar(max)")]
    public string? FotoPerfilBase64 { get; set; }

    /// <summary>
    /// Ruta del documento IAE subido por el usuario.
    /// </summary>
    [StringLength(500)]
    [Unicode(false)]
    public string? DocumentoIAE { get; set; }

    // Relaciones
    [ForeignKey(nameof(IdEmpresa))]
    [InverseProperty("Usuarios")]
    public virtual Empresa? Empresa { get; set; }

    [InverseProperty("Usuario")]
    public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

    [InverseProperty("Usuario")]
    public virtual ICollection<Puja> Pujas { get; set; } = new List<Puja>();
}
