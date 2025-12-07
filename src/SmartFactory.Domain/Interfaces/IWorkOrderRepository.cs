using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Domain.Interfaces;

/// <summary>
/// Repository interface for WorkOrder entity operations.
/// </summary>
public interface IWorkOrderRepository : IRepository<WorkOrder>
{
    Task<WorkOrder?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkOrder>> GetByFactoryAsync(Guid factoryId, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkOrder>> GetByStatusAsync(WorkOrderStatus status, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkOrder>> GetByDateRangeAsync(DateTimeRange dateRange, CancellationToken cancellationToken = default);
    Task<IEnumerable<WorkOrder>> GetActiveWorkOrdersAsync(Guid? factoryId = null, CancellationToken cancellationToken = default);
    Task<WorkOrder?> GetWithStepsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductionSummary> GetProductionSummaryAsync(Guid? factoryId, DateTime date, CancellationToken cancellationToken = default);
}

/// <summary>
/// Production summary for a given period.
/// </summary>
public record ProductionSummary
{
    public DateTime Date { get; init; }
    public int TotalWorkOrders { get; init; }
    public int CompletedWorkOrders { get; init; }
    public int InProgressWorkOrders { get; init; }
    public int TargetUnits { get; init; }
    public int CompletedUnits { get; init; }
    public int DefectUnits { get; init; }
    public double YieldRate => TargetUnits > 0 ? (double)(CompletedUnits - DefectUnits) / TargetUnits * 100 : 0;
}
