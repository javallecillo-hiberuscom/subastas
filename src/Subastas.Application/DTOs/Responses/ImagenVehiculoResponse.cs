namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta con información de imagen de vehículo.
/// </summary>
public class ImagenVehiculoResponse
{
    public int IdImagen { get; set; }
    public int IdVehiculo { get; set; }
    public string? Nombre { get; set; }
    public string? Ruta { get; set; }
    public bool Activo { get; set; }
}
