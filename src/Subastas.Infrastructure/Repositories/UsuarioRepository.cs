using Microsoft.EntityFrameworkCore;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Domain.Entities;
using Subastas.Infrastructure.Data;

namespace Subastas.Infrastructure.Repositories;

/// <summary>
/// Repositorio específico para operaciones con usuarios.
/// </summary>
public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(SubastaContext context) : base(context)
    {
    }

    public async Task<Usuario?> GetByEmailAsync(string email)
    {
        return await _dbSet
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetByEmpresaAsync(int idEmpresa)
    {
        return await _dbSet
            .Where(u => u.IdEmpresa == idEmpresa)
            .Include(u => u.Empresa)
            .ToListAsync();
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _dbSet.AnyAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<Usuario>> GetByRolAsync(string rol)
    {
        return await _dbSet
            .Where(u => u.Rol == rol)
            .Include(u => u.Empresa)
            .ToListAsync();
    }

    public override async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(u => u.Empresa)
            .FirstOrDefaultAsync(u => u.IdUsuario == id);
    }
}
