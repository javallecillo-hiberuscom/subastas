using System.ComponentModel.DataAnnotations;

namespace Subastas.Application.DTOs.Requests;

/// <summary>
/// DTO para crear una nueva subasta.
/// </summary>
public class CrearSubastaRequest
{
    /// <summary>
    /// ID del vehículo a subastar.
    /// </summary>
    [Required(ErrorMessage = "El ID del vehículo es requerido.")]
    public int IdVehiculo { get; set; }

    /// <summary>
    /// Fecha y hora de inicio de la subasta.
    /// </summary>
    [Required(ErrorMessage = "La fecha de inicio es requerida.")]
    public DateTime FechaInicio { get; set; }

    /// <summary>
    /// Fecha y hora de finalización de la subasta.
    /// </summary>
    [Required(ErrorMessage = "La fecha de fin es requerida.")]
    public DateTime FechaFin { get; set; }

    /// <summary>
    /// Precio inicial de la subasta.
    /// </summary>
    [Required(ErrorMessage = "El precio inicial es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio inicial debe ser mayor a 0.")]
    public decimal PrecioInicial { get; set; }

    /// <summary>
    /// Incremento mínimo por puja.
    /// </summary>
    [Required(ErrorMessage = "El incremento mínimo es requerido.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El incremento mínimo debe ser mayor a 0.")]
    public decimal IncrementoMinimo { get; set; }
}
