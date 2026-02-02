using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Subastas.Domain.Entities;

/// <summary>
/// Representa un vehículo disponible para subasta.
/// </summary>
[Table("Vehiculo")]
[Index(nameof(Matricula), Name = "UQ__Vehiculo__30962D1527B6AAEF", IsUnique = true)]
public class Vehiculo
{
    /// <summary>
    /// Identificador único del vehículo.
    /// </summary>
    [Key]
    public int IdVehiculo { get; set; }

    /// <summary>
    /// Marca del vehículo.
    /// </summary>
    [StringLength(50)]
    [Unicode(false)]
    public string Marca { get; set; } = null!;

    /// <summary>
    /// Modelo del vehículo.
    /// </summary>
    [StringLength(50)]
    [Unicode(false)]
    public string Modelo { get; set; } = null!;

    /// <summary>
    /// Matrícula del vehículo (única).
    /// </summary>
    [StringLength(20)]
    [Unicode(false)]
    public string Matricula { get; set; } = null!;

    /// <summary>
    /// Color del vehículo.
    /// </summary>
    [StringLength(30)]
    [Unicode(false)]
    public string? Color { get; set; }

    /// <summary>
    /// Año de fabricación del vehículo.
    /// </summary>
    public int? Anio { get; set; }

    /// <summary>
    /// Número de puertas del vehículo.
    /// </summary>
    public int? NumeroPuertas { get; set; }

    /// <summary>
    /// Kilometraje del vehículo.
    /// </summary>
    public int? Kilometraje { get; set; }

    /// <summary>
    /// Potencia del motor en caballos de fuerza.
    /// </summary>
    public int? Potencia { get; set; }

    /// <summary>
    /// Tipo de motor (Gasolina, Diésel, Eléctrico, Híbrido).
    /// </summary>
    [StringLength(255)]
    public string TipoMotor { get; set; } = null!;

    /// <summary>
    /// Tipo de carrocería (Sedán, SUV, Deportivo, etc.).
    /// </summary>
    [StringLength(255)]
    public string TipoCarroceria { get; set; } = null!;

    /// <summary>
    /// Descripción adicional del vehículo.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Fecha de matriculación del vehículo.
    /// </summary>
    public DateOnly? FechaMatriculacion { get; set; }

    /// <summary>
    /// Fecha de creación del registro.
    /// </summary>
    public DateOnly FechaCreacion { get; set; }

    /// <summary>
    /// Estado del vehículo (Disponible, En Subasta, Vendido).
    /// </summary>
    [StringLength(255)]
    public string Estado { get; set; } = null!;

    // Relaciones
    [InverseProperty("Vehiculo")]
    public virtual ICollection<ImagenVehiculo> ImagenesVehiculo { get; set; } = new List<ImagenVehiculo>();

    [InverseProperty("Vehiculo")]
    public virtual ICollection<Subasta> Subastas { get; set; } = new List<Subasta>();
}
