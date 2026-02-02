namespace Subastas.Application.DTOs.Responses;

/// <summary>
/// DTO de respuesta con información de vehículo.
/// </summary>
public class VehiculoResponse
{
    public int IdVehiculo { get; set; }
    public string Marca { get; set; } = null!;
    public string Modelo { get; set; } = null!;
    public string Matricula { get; set; } = null!;
    public string? Color { get; set; }
    public int? Anio { get; set; }
    public int? NumeroPuertas { get; set; }
    public int? Kilometraje { get; set; }
    public int? Potencia { get; set; }
    public string TipoMotor { get; set; } = null!;
    public string TipoCarroceria { get; set; } = null!;
    public string? Descripcion { get; set; }
    public DateOnly? FechaMatriculacion { get; set; }
    public DateOnly FechaCreacion { get; set; }
    public string Estado { get; set; } = null!;
    
    // Imágenes asociadas
    public List<ImagenVehiculoResponse>? Imagenes { get; set; }
}
