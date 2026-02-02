using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa una imagen asociada a un vehículo.
/// </summary>
[Table("ImagenVehiculo")]
public class ImagenVehiculo
{
    /// <summary>
    /// Identificador único de la imagen.
    /// </summary>
    [Key]
    public int IdImagen { get; set; }

    /// <summary>
    /// Identificador del vehículo al que pertenece la imagen.
    /// </summary>
    public int IdVehiculo { get; set; }

    /// <summary>
    /// Nombre del archivo de imagen.
    /// </summary>
    [StringLength(100)]
    [Unicode(false)]
    public string? Nombre { get; set; }

    /// <summary>
    /// Ruta de almacenamiento de la imagen.
    /// </summary>
    [StringLength(200)]
    [Unicode(false)]
    public string? Ruta { get; set; }

    /// <summary>
    /// Indica si la imagen está activa (0 = no, 1 = sí).
    /// </summary>
    public byte Activo { get; set; }

    // Relaciones
    [ForeignKey(nameof(IdVehiculo))]
    [InverseProperty("ImagenesVehiculo")]
    public virtual Vehiculo Vehiculo { get; set; } = null!;
}
