using SmartFactory.Domain.Entities;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for ProductionLine entity operations.
/// </summary>
public interface IProductionLineRepository : IRepository<ProductionLine>
{
    /// <summary>
    /// Gets production lines for a specific factory.
    /// </summary>
    Task<IEnumerable<ProductionLine>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a production line with its equipment.
    /// </summary>
    Task<ProductionLine?> GetWithEquipmentAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all production lines with their equipment.
    /// </summary>
    Task<IEnumerable<ProductionLine>> GetAllWithEquipmentAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active production lines for a factory.
    /// </summary>
    Task<IEnumerable<ProductionLine>> GetActiveByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a production line code exists within a factory.
    /// </summary>
    Task<bool> CodeExistsAsync(Guid factoryId, string code, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
