using Microsoft.EntityFrameworkCore;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Domain.Entities;
using Subastas.Infrastructure.Data;

namespace Subastas.Infrastructure.Repositories;

/// <summary>
/// Repositorio específico para operaciones con subastas.
/// </summary>
public class SubastaRepository : Repository<Subasta>, ISubastaRepository
{
    public SubastaRepository(SubastaContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Subasta>> GetSubastasActivasAsync()
    {
        return await _dbSet
            .Where(s => s.Estado == "activa" && s.FechaFin > DateTime.Now)
            .Include(s => s.Vehiculo)
            .OrderBy(s => s.FechaFin)
            .ToListAsync();
    }

    public async Task<IEnumerable<Subasta>> GetByEstadoAsync(string estado)
    {
        return await _dbSet
            .Where(s => s.Estado == estado)
            .Include(s => s.Vehiculo)
            .ToListAsync();
    }

    public async Task<Subasta?> GetByIdWithPujasAsync(int idSubasta)
    {
        return await _dbSet
            .Include(s => s.Vehiculo)
            .Include(s => s.Pujas)
                .ThenInclude(p => p.Usuario)
            .FirstOrDefaultAsync(s => s.IdSubasta == idSubasta);
    }

    public async Task<IEnumerable<Subasta>> GetByVehiculoAsync(int idVehiculo)
    {
        return await _dbSet
            .Where(s => s.IdVehiculo == idVehiculo)
            .Include(s => s.Vehiculo)
            .ToListAsync();
    }

    public override async Task<Subasta?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(s => s.Vehiculo)
            .FirstOrDefaultAsync(s => s.IdSubasta == id);
    }
}
