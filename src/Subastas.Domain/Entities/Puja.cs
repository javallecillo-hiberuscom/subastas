using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa una puja realizada por un usuario en una subasta.
/// </summary>
[Table("Puja")]
public class Puja
{
    /// <summary>
    /// Identificador único de la puja.
    /// </summary>
    [Key]
    public int IdPuja { get; set; }

    /// <summary>
    /// Identificador de la subasta asociada.
    /// </summary>
    public int IdSubasta { get; set; }

    /// <summary>
    /// Identificador del usuario que realiza la puja.
    /// </summary>
    public int IdUsuario { get; set; }

    /// <summary>
    /// Cantidad ofrecida en la puja.
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Fecha y hora en que se realizó la puja.
    /// </summary>
    [Precision(0)]
    public DateTime FechaPuja { get; set; }

    // Relaciones
    [ForeignKey(nameof(IdSubasta))]
    [InverseProperty("Pujas")]
    public virtual Subasta Subasta { get; set; } = null!;

    [ForeignKey(nameof(IdUsuario))]
    [InverseProperty("Pujas")]
    public virtual Usuario Usuario { get; set; } = null!;

    [InverseProperty("Puja")]
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
