using Subastas.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Subastas.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para operaciones específicas del repositorio de subastas.
/// </summary>
public interface ISubastaRepository : IRepository<Subasta>
{
    /// <summary>
    /// Obtiene subastas activas.
    /// </summary>
    Task<IEnumerable<Subasta>> GetSubastasActivasAsync();

    /// <summary>
    /// Obtiene subastas por estado.
    /// </summary>
    Task<IEnumerable<Subasta>> GetByEstadoAsync(string estado);

    /// <summary>
    /// Obtiene una subasta con sus pujas incluidas.
    /// </summary>
    Task<Subasta?> GetByIdWithPujasAsync(int idSubasta);

    /// <summary>
    /// Obtiene subastas de un vehículo específico.
    /// </summary>
    Task<IEnumerable<Subasta>> GetByVehiculoAsync(int idVehiculo);

    /// <summary>
    /// Obtiene subastas entre dos fechas (inclusive).
    /// </summary>
    Task<IEnumerable<Subasta>> GetBetweenDatesAsync(DateTime from, DateTime to);
}
