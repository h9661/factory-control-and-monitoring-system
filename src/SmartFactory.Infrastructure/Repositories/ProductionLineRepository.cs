using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for ProductionLine entity.
/// </summary>
public class ProductionLineRepository : RepositoryBase<ProductionLine>, IProductionLineRepository
{
    public ProductionLineRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ProductionLine>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(pl => pl.FactoryId == factoryId)
            .OrderBy(pl => pl.Sequence)
            .ToListAsync(cancellationToken);
    }

    public async Task<ProductionLine?> GetWithEquipmentAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pl => pl.Equipment)
            .FirstOrDefaultAsync(pl => pl.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<ProductionLine>> GetAllWithEquipmentAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(pl => pl.Equipment)
            .OrderBy(pl => pl.Sequence)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<ProductionLine>> GetActiveByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(pl => pl.FactoryId == factoryId && pl.IsActive)
            .OrderBy(pl => pl.Sequence)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(Guid factoryId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(pl => pl.FactoryId == factoryId && pl.Code == code.ToUpperInvariant());

        if (excludeId.HasValue)
        {
            query = query.Where(pl => pl.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
