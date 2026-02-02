using System.ComponentModel.DataAnnotations;

namespace Subastas.Application.DTOs.Requests;

/// <summary>
/// DTO para realizar una puja en una subasta.
/// </summary>
public class CrearPujaRequest
{
    /// <summary>
    /// ID de la subasta en la que se realiza la puja.
    /// </summary>
    [Required(ErrorMessage = "El ID de la subasta es requerido.")]
    public int IdSubasta { get; set; }

    /// <summary>
    /// ID del usuario que realiza la puja.
    /// </summary>
    [Required(ErrorMessage = "El ID del usuario es requerido.")]
    public int IdUsuario { get; set; }

    /// <summary>
    /// Cantidad ofrecida en la puja.
    /// </summary>
    [Required(ErrorMessage = "La cantidad es requerida.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public decimal Cantidad { get; set; }
}
