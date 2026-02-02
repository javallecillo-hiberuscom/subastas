using Subastas.Domain.Entities;

namespace Subastas.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para operaciones específicas del repositorio de pujas.
/// </summary>
public interface IPujaRepository : IRepository<Puja>
{
    /// <summary>
    /// Obtiene todas las pujas de una subasta.
    /// </summary>
    Task<IEnumerable<Puja>> GetBySubastaAsync(int idSubasta);

    /// <summary>
    /// Obtiene todas las pujas de un usuario.
    /// </summary>
    Task<IEnumerable<Puja>> GetByUsuarioAsync(int idUsuario);

    /// <summary>
    /// Obtiene la puja más alta de una subasta.
    /// </summary>
    Task<Puja?> GetPujaMasAltaAsync(int idSubasta);

    /// <summary>
    /// Obtiene las últimas N pujas de una subasta.
    /// </summary>
    Task<IEnumerable<Puja>> GetUltimasPujasAsync(int idSubasta, int cantidad);
}
