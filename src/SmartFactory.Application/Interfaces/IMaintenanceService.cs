using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.Maintenance;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for maintenance operations.
/// </summary>
public interface IMaintenanceService
{
    /// <summary>
    /// Gets a paginated list of maintenance records.
    /// </summary>
    Task<PagedResult<MaintenanceRecordDto>> GetMaintenanceRecordsAsync(MaintenanceFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance record by ID.
    /// </summary>
    Task<MaintenanceRecordDetailDto?> GetMaintenanceRecordByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Schedules new maintenance.
    /// </summary>
    Task<MaintenanceRecordDto> ScheduleMaintenanceAsync(MaintenanceCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts maintenance.
    /// </summary>
    Task StartMaintenanceAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes maintenance.
    /// </summary>
    Task CompleteMaintenanceAsync(Guid id, MaintenanceCompleteDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels maintenance.
    /// </summary>
    Task CancelMaintenanceAsync(Guid id, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reschedules maintenance.
    /// </summary>
    Task RescheduleMaintenanceAsync(Guid id, MaintenanceRescheduleDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue maintenance.
    /// </summary>
    Task<IEnumerable<MaintenanceDueAlertDto>> GetOverdueMaintenanceAsync(Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming maintenance.
    /// </summary>
    Task<IEnumerable<MaintenanceDueAlertDto>> GetUpcomingMaintenanceAsync(int days, Guid? factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance summary.
    /// </summary>
    Task<MaintenanceSummaryDto> GetMaintenanceSummaryAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
}
