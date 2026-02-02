namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta con información de puja.
/// </summary>
public class PujaResponse
{
    public int IdPuja { get; set; }
    public int IdSubasta { get; set; }
    public int IdUsuario { get; set; }
    public decimal Cantidad { get; set; }
    public DateTime FechaPuja { get; set; }
    
    // Información del usuario
    public string? NombreUsuario { get; set; }
    public string? EmailUsuario { get; set; }
}
