using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for Equipment entity operations.
/// </summary>
public interface IEquipmentRepository : IRepository<Equipment>
{
    Task<IEnumerable<Equipment>> GetByProductionLineAsync(Guid lineId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipment>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipment>> GetByStatusAsync(EquipmentStatus status, CancellationToken cancellationToken = default);
    Task<Equipment?> GetWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Equipment>> GetEquipmentDueForMaintenanceAsync(CancellationToken cancellationToken = default);
    Task<EquipmentStatusSummary> GetStatusSummaryAsync(Guid? factoryId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary of equipment status counts.
/// </summary>
public record EquipmentStatusSummary
{
    public int TotalCount { get; init; }
    public int RunningCount { get; init; }
    public int IdleCount { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public int MaintenanceCount { get; init; }
    public int OfflineCount { get; init; }
}
