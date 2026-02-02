using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa una empresa participante en el sistema de subastas.
/// </summary>
[Table("Empresa")]
[Index(nameof(Cif), Name = "UQ__Empresa__D837D05C040BA45A", IsUnique = true)]
public class Empresa
{
    /// <summary>
    /// Identificador único de la empresa.
    /// </summary>
    [Key]
    public int IdEmpresa { get; set; }

    /// <summary>
    /// Nombre de la empresa.
    /// </summary>
    [StringLength(100)]
    [Unicode(false)]
    public string Nombre { get; set; } = null!;

    /// <summary>
    /// CIF de la empresa (único).
    /// </summary>
    [StringLength(20)]
    [Unicode(false)]
    public string Cif { get; set; } = null!;

    /// <summary>
    /// Dirección de la empresa.
    /// </summary>
    [StringLength(200)]
    [Unicode(false)]
    public string? Direccion { get; set; }

    /// <summary>
    /// Teléfono de contacto de la empresa.
    /// </summary>
    [StringLength(20)]
    [Unicode(false)]
    public string? Telefono { get; set; }

    // Relaciones
    [InverseProperty("Empresa")]
    public virtual ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
