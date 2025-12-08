using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Factory;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for factory operations.
/// </summary>
public interface IFactoryService
{
    /// <summary>
    /// Gets all factories.
    /// </summary>
    Task<IEnumerable<FactoryDto>> GetAllFactoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active factories.
    /// </summary>
    Task<IEnumerable<FactoryDto>> GetActiveFactoriesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets factory by ID.
    /// </summary>
    Task<FactoryDetailDto?> GetFactoryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new factory.
    /// </summary>
    Task<FactoryDto> CreateFactoryAsync(FactoryCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a factory.
    /// </summary>
    Task UpdateFactoryAsync(Guid id, FactoryUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a factory (soft delete).
    /// </summary>
    Task DeleteFactoryAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets production lines for a factory.
    /// </summary>
    Task<IEnumerable<ProductionLineDto>> GetProductionLinesAsync(Guid factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a production line.
    /// </summary>
    Task<ProductionLineDto> CreateProductionLineAsync(ProductionLineCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a production line.
    /// </summary>
    Task UpdateProductionLineAsync(Guid id, ProductionLineUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a production line.
    /// </summary>
    Task DeleteProductionLineAsync(Guid id, CancellationToken cancellationToken = default);
}
