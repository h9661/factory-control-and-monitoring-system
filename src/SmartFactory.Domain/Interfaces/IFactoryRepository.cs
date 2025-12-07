using SmartFactory.Domain.Entities;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for Factory entity operations.
/// </summary>
public interface IFactoryRepository : IRepository<Factory>
{
    Task<Factory?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IEnumerable<Factory>> GetActiveFactoriesAsync(CancellationToken cancellationToken = default);
    Task<Factory?> GetWithProductionLinesAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken = default);
}
