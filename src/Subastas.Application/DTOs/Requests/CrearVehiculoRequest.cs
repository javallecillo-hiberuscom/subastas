using System.ComponentModel.DataAnnotations;

namespace Subastas.Application.DTOs.Requests;

/// <summary>
/// DTO para crear o registrar un vehículo.
/// </summary>
public class CrearVehiculoRequest
{
    /// <summary>
    /// Marca del vehículo.
    /// </summary>
    [Required(ErrorMessage = "La marca es requerida.")]
    [StringLength(50)]
    public string Marca { get; set; } = null!;

    /// <summary>
    /// Modelo del vehículo.
    /// </summary>
    [Required(ErrorMessage = "El modelo es requerido.")]
    [StringLength(50)]
    public string Modelo { get; set; } = null!;

    /// <summary>
    /// Matrícula del vehículo.
    /// </summary>
    [Required(ErrorMessage = "La matrícula es requerida.")]
    [StringLength(20)]
    public string Matricula { get; set; } = null!;

    /// <summary>
    /// Color del vehículo.
    /// </summary>
    [StringLength(30)]
    public string? Color { get; set; }

    /// <summary>
    /// Año de fabricación.
    /// </summary>
    [Range(1900, 2100, ErrorMessage = "El año debe estar entre 1900 y 2100.")]
    public int? Anio { get; set; }

    /// <summary>
    /// Número de puertas.
    /// </summary>
    [Range(2, 10, ErrorMessage = "El número de puertas debe estar entre 2 y 10.")]
    public int? NumeroPuertas { get; set; }

    /// <summary>
    /// Kilometraje del vehículo.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "El kilometraje debe ser mayor o igual a 0.")]
    public int? Kilometraje { get; set; }

    /// <summary>
    /// Potencia del motor en CV.
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "La potencia debe ser mayor o igual a 0.")]
    public int? Potencia { get; set; }

    /// <summary>
    /// Tipo de motor (Gasolina, Diésel, Eléctrico, Híbrido).
    /// </summary>
    [Required(ErrorMessage = "El tipo de motor es requerido.")]
    [StringLength(255)]
    public string TipoMotor { get; set; } = null!;

    /// <summary>
    /// Tipo de carrocería (Sedán, SUV, Deportivo, etc.).
    /// </summary>
    [Required(ErrorMessage = "El tipo de carrocería es requerido.")]
    [StringLength(255)]
    public string TipoCarroceria { get; set; } = null!;

    /// <summary>
    /// Descripción adicional del vehículo.
    /// </summary>
    public string? Descripcion { get; set; }

    /// <summary>
    /// Fecha de matriculación.
    /// </summary>
    public DateOnly? FechaMatriculacion { get; set; }
}
