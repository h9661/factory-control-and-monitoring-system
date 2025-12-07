using Microsoft.EntityFrameworkCore;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Infrastructure.Data;

namespace SmartFactory.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for Factory entity.
/// </summary>
public class FactoryRepository : RepositoryBase<Factory>, IFactoryRepository
{
    public FactoryRepository(SmartFactoryDbContext context) : base(context)
    {
    }

    public async Task<Factory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.Code == code.ToUpperInvariant(), cancellationToken);
    }

    public async Task<IEnumerable<Factory>> GetActiveFactoriesAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Factory?> GetWithProductionLinesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(f => f.ProductionLines)
                .ThenInclude(pl => pl.Equipment)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(f => f.Code == code.ToUpperInvariant(), cancellationToken);
    }
}
