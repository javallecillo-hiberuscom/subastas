using Microsoft.EntityFrameworkCore;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Domain.Entities;
using Subastas.Infrastructure.Data;

namespace Subastas.Infrastructure.Repositories;

/// <summary>
/// Repositorio específico para operaciones con pujas.
/// </summary>
public class PujaRepository : Repository<Puja>, IPujaRepository
{
    public PujaRepository(SubastaContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Puja>> GetBySubastaAsync(int idSubasta)
    {
        return await _dbSet
            .Where(p => p.IdSubasta == idSubasta)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.FechaPuja)
            .ToListAsync();
    }

    public async Task<IEnumerable<Puja>> GetByUsuarioAsync(int idUsuario)
    {
        return await _dbSet
            .Where(p => p.IdUsuario == idUsuario)
            .Include(p => p.Subasta)
                .ThenInclude(s => s.Vehiculo)
            .OrderByDescending(p => p.FechaPuja)
            .ToListAsync();
    }

    public async Task<Puja?> GetPujaMasAltaAsync(int idSubasta)
    {
        return await _dbSet
            .Where(p => p.IdSubasta == idSubasta)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.Cantidad)
            .FirstOrDefaultAsync();
    }

    public async Task<IEnumerable<Puja>> GetUltimasPujasAsync(int idSubasta, int cantidad)
    {
        return await _dbSet
            .Where(p => p.IdSubasta == idSubasta)
            .Include(p => p.Usuario)
            .OrderByDescending(p => p.FechaPuja)
            .Take(cantidad)
            .ToListAsync();
    }
}
