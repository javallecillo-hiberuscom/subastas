using Subastas.Domain.Entities;

namespace Subastas.Application.Interfaces.Repositories;

/// <summary>
/// Interfaz para operaciones específicas del repositorio de usuarios.
/// </summary>
public interface IUsuarioRepository : IRepository<Usuario>
{
    /// <summary>
    /// Busca un usuario por su email.
    /// </summary>
    Task<Usuario?> GetByEmailAsync(string email);

    /// <summary>
    /// Obtiene todos los usuarios de una empresa.
    /// </summary>
    Task<IEnumerable<Usuario>> GetByEmpresaAsync(int idEmpresa);

    /// <summary>
    /// Verifica si un email ya está registrado.
    /// </summary>
    Task<bool> EmailExistsAsync(string email);

    /// <summary>
    /// Obtiene usuarios por rol.
    /// </summary>
    Task<IEnumerable<Usuario>> GetByRolAsync(string rol);
}
