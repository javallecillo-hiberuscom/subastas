using Subastas.Domain.Entities;

namespace Subastas.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para operaciones de repositorio genérico.
/// </summary>
/// <typeparam name="T">Tipo de entidad</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>
    /// Obtiene todas las entidades.
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync();

    /// <summary>
    /// Obtiene una entidad por su ID.
    /// </summary>
    Task<T?> GetByIdAsync(int id);

    /// <summary>
    /// Añade una nueva entidad.
    /// </summary>
    Task<T> AddAsync(T entity);

    /// <summary>
    /// Actualiza una entidad existente.
    /// </summary>
    Task UpdateAsync(T entity);

    /// <summary>
    /// Elimina una entidad.
    /// </summary>
    Task DeleteAsync(T entity);

    /// <summary>
    /// Guarda los cambios en la base de datos.
    /// </summary>
    Task<int> SaveChangesAsync();
}
