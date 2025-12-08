using SmartFactory.Application.DTOs.Alarm;
using SmartFactory.Application.DTOs.Common;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for alarm operations.
/// </summary>
public interface IAlarmService
{
    /// <summary>
    /// Gets a paginated list of alarms.
    /// </summary>
    Task<PagedResult<AlarmDto>> GetAlarmsAsync(AlarmFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alarm by ID.
    /// </summary>
    Task<AlarmDetailDto?> GetAlarmByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active alarms.
    /// </summary>
    Task<IEnumerable<AlarmDto>> GetActiveAlarmsAsync(Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent alarms.
    /// </summary>
    Task<IEnumerable<AlarmDto>> GetRecentAlarmsAsync(int count, Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new alarm.
    /// </summary>
    Task<AlarmDto> CreateAlarmAsync(AlarmCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges an alarm.
    /// </summary>
    Task AcknowledgeAlarmAsync(Guid id, AlarmAcknowledgeDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Acknowledges multiple alarms.
    /// </summary>
    Task AcknowledgeAlarmsAsync(IEnumerable<Guid> ids, string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves an alarm.
    /// </summary>
    Task ResolveAlarmAsync(Guid id, AlarmResolveDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets alarm summary.
    /// </summary>
    Task<AlarmSummaryDto> GetAlarmSummaryAsync(Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active alarm count.
    /// </summary>
    Task<int> GetActiveAlarmCountAsync(Guid? factoryId, CancellationToken cancellationToken = default);
}
