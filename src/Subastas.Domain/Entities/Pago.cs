using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa un pago realizado por una puja ganada.
/// </summary>
[Table("Pago")]
public class Pago
{
    /// <summary>
    /// Identificador único del pago.
    /// </summary>
    [Key]
    public int IdPago { get; set; }

    /// <summary>
    /// Identificador de la puja asociada al pago.
    /// </summary>
    public int IdPuja { get; set; }

    /// <summary>
    /// Fecha y hora en que se realizó el pago.
    /// </summary>
    [Precision(0)]
    public DateTime FechaPago { get; set; }

    /// <summary>
    /// Cantidad pagada.
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal Cantidad { get; set; }

    /// <summary>
    /// Método de pago utilizado (Tarjeta, Transferencia, etc.).
    /// </summary>
    [StringLength(255)]
    public string MetodoPago { get; set; } = null!;

    // Relaciones
    [ForeignKey(nameof(IdPuja))]
    [InverseProperty("Pagos")]
    public virtual Puja Puja { get; set; } = null!;
}
