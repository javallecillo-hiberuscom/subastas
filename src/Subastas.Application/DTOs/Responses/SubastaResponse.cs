namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta con información de subasta.
/// </summary>
public class SubastaResponse
{
    public int IdSubasta { get; set; }
    public int IdVehiculo { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public decimal PrecioInicial { get; set; }
    public decimal IncrementoMinimo { get; set; }
    public decimal? PrecioActual { get; set; }
    public string Estado { get; set; } = null!;
    
    // Información del vehículo
    public VehiculoResponse? Vehiculo { get; set; }
    
    // Pujas asociadas
    public List<PujaResponse>? Pujas { get; set; }
}
