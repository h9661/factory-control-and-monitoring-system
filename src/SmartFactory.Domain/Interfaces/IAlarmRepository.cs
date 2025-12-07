using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for Alarm entity operations.
/// </summary>
public interface IAlarmRepository : IRepository<Alarm>
{
    Task<IEnumerable<Alarm>> GetActiveAlarmsAsync(Guid? factoryId = null, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alarm>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alarm>> GetBySeverityAsync(AlarmSeverity severity, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alarm>> GetByDateRangeAsync(DateTimeRange dateRange, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alarm>> GetRecentAlarmsAsync(int count, Guid? factoryId = null, CancellationToken cancellationToken = default);
    Task<int> GetActiveAlarmCountAsync(Guid? factoryId = null, CancellationToken cancellationToken = default);
    Task<AlarmSummary> GetAlarmSummaryAsync(Guid? factoryId = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Summary of alarm statistics.
/// </summary>
public record AlarmSummary
{
    public int TotalActive { get; init; }
    public int CriticalCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InformationCount { get; init; }
    public int AcknowledgedCount { get; init; }
    public int UnacknowledgedCount { get; init; }
}
