using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for QualityRecord entity operations.
/// </summary>
public interface IQualityRecordRepository : IRepository<QualityRecord>
{
    /// <summary>
    /// Gets quality records for a specific equipment.
    /// </summary>
    Task<IEnumerable<QualityRecord>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality records for a specific work order.
    /// </summary>
    Task<IEnumerable<QualityRecord>> GetByWorkOrderAsync(Guid workOrderId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality records within a date range.
    /// </summary>
    Task<IEnumerable<QualityRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality records by inspection result.
    /// </summary>
    Task<IEnumerable<QualityRecord>> GetByResultAsync(InspectionResult result, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality records for a factory.
    /// </summary>
    Task<IEnumerable<QualityRecord>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets quality record with related entities.
    /// </summary>
    Task<QualityRecord?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets defect statistics for a date range.
    /// </summary>
    Task<DefectStatistics> GetDefectStatisticsAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}

/// <summary>
/// Defect statistics summary.
/// </summary>
public class DefectStatistics
{
    public int TotalInspections { get; set; }
    public int PassCount { get; set; }
    public int FailCount { get; set; }
    public int TotalDefects { get; set; }
    public double PassRate { get; set; }
    public Dictionary<DefectType, int> DefectsByType { get; set; } = new();
}
