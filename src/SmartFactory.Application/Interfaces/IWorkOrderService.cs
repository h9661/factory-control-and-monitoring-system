using SmartFactory.Application.DTOs.Common;
using SmartFactory.Application.DTOs.WorkOrder;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Service interface for work order operations.
/// </summary>
public interface IWorkOrderService
{
    /// <summary>
    /// Gets a paginated list of work orders.
    /// </summary>
    Task<PagedResult<WorkOrderDto>> GetWorkOrdersAsync(WorkOrderFilterDto filter, PaginationDto pagination, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets work order by ID.
    /// </summary>
    Task<WorkOrderDetailDto?> GetWorkOrderByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets work order by order number.
    /// </summary>
    Task<WorkOrderDto?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates new work order.
    /// </summary>
    Task<WorkOrderDto> CreateWorkOrderAsync(WorkOrderCreateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates existing work order.
    /// </summary>
    Task UpdateWorkOrderAsync(Guid id, WorkOrderUpdateDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts a work order.
    /// </summary>
    Task StartWorkOrderAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Pauses a work order.
    /// </summary>
    Task PauseWorkOrderAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes a paused work order.
    /// </summary>
    Task ResumeWorkOrderAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Completes a work order.
    /// </summary>
    Task CompleteWorkOrderAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancels a work order.
    /// </summary>
    Task CancelWorkOrderAsync(Guid id, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reports work order progress.
    /// </summary>
    Task ReportProgressAsync(Guid id, WorkOrderProgressDto dto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets production summary for a date.
    /// </summary>
    Task<ProductionSummaryDto> GetProductionSummaryAsync(Guid? factoryId, DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active work orders.
    /// </summary>
    Task<IEnumerable<WorkOrderDto>> GetActiveWorkOrdersAsync(Guid? factoryId, CancellationToken cancellationToken = default);
}
