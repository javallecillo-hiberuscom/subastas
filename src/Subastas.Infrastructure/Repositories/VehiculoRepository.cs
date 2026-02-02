using Microsoft.EntityFrameworkCore;
using Subastas.Application.Interfaces.Repositories;
using Subastas.Domain.Entities;
using Subastas.Infrastructure.Data;

namespace Subastas.Infrastructure.Repositories;

/// <summary>
/// Repositorio específico para operaciones con vehículos.
/// </summary>
public class VehiculoRepository : Repository<Vehiculo>, IVehiculoRepository
{
    public VehiculoRepository(SubastaContext context) : base(context)
    {
    }

    public async Task<Vehiculo?> GetByMatriculaAsync(string matricula)
    {
        return await _dbSet
            .Include(v => v.ImagenesVehiculo)
            .FirstOrDefaultAsync(v => v.Matricula == matricula);
    }

    public async Task<IEnumerable<Vehiculo>> GetByEstadoAsync(string estado)
    {
        return await _dbSet
            .Where(v => v.Estado == estado)
            .Include(v => v.ImagenesVehiculo)
            .ToListAsync();
    }

    public async Task<Vehiculo?> GetByIdWithImagenesAsync(int idVehiculo)
    {
        return await _dbSet
            .Include(v => v.ImagenesVehiculo.Where(i => i.Activo == 1))
            .FirstOrDefaultAsync(v => v.IdVehiculo == idVehiculo);
    }

    public async Task<bool> MatriculaExistsAsync(string matricula)
    {
        return await _dbSet.AnyAsync(v => v.Matricula == matricula);
    }

    public override async Task<Vehiculo?> GetByIdAsync(int id)
    {
        return await _dbSet
            .Include(v => v.ImagenesVehiculo)
            .FirstOrDefaultAsync(v => v.IdVehiculo == id);
    }
}
