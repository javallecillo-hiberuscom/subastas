using Subastas.Application.DTOs.Requests;
using Subastas.Application.DTOs.Responses;

namespace Subastas.Application.Interfaces.Services;

/// <summary>
/// Contrato para el servicio de pujas.
/// </summary>
public interface IPujaService
{
    /// <summary>
    /// Obtiene todas las pujas del sistema.
    /// </summary>
    Task<IEnumerable<PujaResponse>> ObtenerTodasAsync();

    /// <summary>
    /// Obtiene las pujas realizadas por un usuario específico.
    /// </summary>
    Task<IEnumerable<PujaResponse>> ObtenerPorUsuarioAsync(int idUsuario);

    /// <summary>
    /// Obtiene las pujas activas (en subastas no finalizadas).
    /// </summary>
    Task<IEnumerable<PujaResponse>> ObtenerActivasAsync();

    /// <summary>
    /// Crea una nueva puja en una subasta.
    /// </summary>
    Task<PujaResponse> CrearPujaAsync(CrearPujaRequest request);
}
