using Subastas.Domain.Entities;

namespace Subastas.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para operaciones específicas del repositorio de vehículos.
/// </summary>
public interface IVehiculoRepository : IRepository<Vehiculo>
{
    /// <summary>
    /// Busca un vehículo por su matrícula.
    /// </summary>
    Task<Vehiculo?> GetByMatriculaAsync(string matricula);

    /// <summary>
    /// Obtiene vehículos por estado.
    /// </summary>
    Task<IEnumerable<Vehiculo>> GetByEstadoAsync(string estado);

    /// <summary>
    /// Obtiene un vehículo con sus imágenes incluidas.
    /// </summary>
    Task<Vehiculo?> GetByIdWithImagenesAsync(int idVehiculo);

    /// <summary>
    /// Verifica si una matrícula ya existe.
    /// </summary>
    Task<bool> MatriculaExistsAsync(string matricula);
}
