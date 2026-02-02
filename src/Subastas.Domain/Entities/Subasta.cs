using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa una subasta de un vehículo.
/// </summary>
[Table("Subasta")]
public class Subasta
{
    /// <summary>
    /// Identificador único de la subasta.
    /// </summary>
    [Key]
    public int IdSubasta { get; set; }

    /// <summary>
    /// Identificador del vehículo en subasta.
    /// </summary>
    public int IdVehiculo { get; set; }

    /// <summary>
    /// Fecha y hora de inicio de la subasta.
    /// </summary>
    [Precision(0)]
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha y hora de finalización de la subasta.
    /// </summary>
    [Precision(0)]
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Precio inicial de la subasta.
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal PrecioInicial { get; set; }

    /// <summary>
    /// Incremento mínimo permitido por puja.
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal IncrementoMinimo { get; set; }

    /// <summary>
    /// Precio actual de la subasta (última puja).
    /// </summary>
    [Column(TypeName = "decimal(10, 2)")]
    public decimal? PrecioActual { get; set; }

    /// <summary>
    /// Estado de la subasta (Activa, Finalizada, Cancelada).
    /// </summary>
    [StringLength(255)]
    public string Estado { get; set; } = null!;

    // Relaciones
    [ForeignKey(nameof(IdVehiculo))]
    [InverseProperty("Subastas")]
    public virtual Vehiculo Vehiculo { get; set; } = null!;

    [InverseProperty("Subasta")]
    public virtual ICollection<Notificacion> Notificaciones { get; set; } = new List<Notificacion>();

    [InverseProperty("Subasta")]
    public virtual ICollection<Puja> Pujas { get; set; } = new List<Puja>();
}
