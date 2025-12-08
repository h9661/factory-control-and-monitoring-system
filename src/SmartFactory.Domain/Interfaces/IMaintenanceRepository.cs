using SmartFactory.Domain.Common;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for MaintenanceRecord entity operations.
/// </summary>
public interface IMaintenanceRepository : IRepository<MaintenanceRecord>
{
    /// <summary>
    /// Gets maintenance records for a specific equipment.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetByEquipmentAsync(Guid equipmentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance records by status.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetByStatusAsync(MaintenanceStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets overdue maintenance records.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetOverdueAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets upcoming maintenance records within specified days.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetUpcomingAsync(int days, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance records within a date range.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance records for a factory.
    /// </summary>
    Task<IEnumerable<MaintenanceRecord>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets maintenance record with equipment details.
    /// </summary>
    Task<MaintenanceRecord?> GetWithEquipmentAsync(Guid id, CancellationToken cancellationToken = default);
}
