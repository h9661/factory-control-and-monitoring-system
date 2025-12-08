using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.DTOs.WorkOrder;

/// <summary>
/// Data transfer object for work order list display.
/// </summary>
public record WorkOrderDto
{
    public Guid Id { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int TargetQuantity { get; init; }
    public int CompletedQuantity { get; init; }
    public int DefectQuantity { get; init; }
    public WorkOrderPriority Priority { get; init; }
    public WorkOrderStatus Status { get; init; }
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public DateTime? ActualStart { get; init; }
    public DateTime? ActualEnd { get; init; }
    public string? CustomerName { get; init; }
    public double YieldRate { get; init; }
    public double CompletionPercentage { get; init; }
    public bool IsOnSchedule { get; init; }
}

/// <summary>
/// Detailed work order information with steps.
/// </summary>
public record WorkOrderDetailDto : WorkOrderDto
{
    public Guid FactoryId { get; init; }
    public string FactoryName { get; init; } = string.Empty;
    public string? CustomerOrderRef { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IEnumerable<WorkOrderStepDto> Steps { get; init; } = Enumerable.Empty<WorkOrderStepDto>();
}

/// <summary>
/// Work order step information.
/// </summary>
public record WorkOrderStepDto
{
    public Guid Id { get; init; }
    public int Sequence { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public WorkOrderStepStatus Status { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
}

/// <summary>
/// DTO for creating new work order.
/// </summary>
public record WorkOrderCreateDto
{
    public Guid FactoryId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int TargetQuantity { get; init; }
    public WorkOrderPriority Priority { get; init; } = WorkOrderPriority.Normal;
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerOrderRef { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for updating work order.
/// </summary>
public record WorkOrderUpdateDto
{
    public string ProductName { get; init; } = string.Empty;
    public int TargetQuantity { get; init; }
    public WorkOrderPriority Priority { get; init; }
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public string? CustomerName { get; init; }
    public string? CustomerOrderRef { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// DTO for reporting work order progress.
/// </summary>
public record WorkOrderProgressDto
{
    public int CompletedQuantity { get; init; }
    public int DefectQuantity { get; init; }
}

/// <summary>
/// Filter criteria for work order queries.
/// </summary>
public record WorkOrderFilterDto
{
    public Guid? FactoryId { get; init; }
    public WorkOrderStatus? Status { get; init; }
    public WorkOrderPriority? Priority { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? SearchText { get; init; }
}

/// <summary>
/// Production summary for dashboard and reports.
/// </summary>
public record ProductionSummaryDto
{
    public DateTime Date { get; init; }
    public int TotalWorkOrders { get; init; }
    public int CompletedWorkOrders { get; init; }
    public int InProgressWorkOrders { get; init; }
    public int TotalTargetUnits { get; init; }
    public int TotalCompletedUnits { get; init; }
    public int TotalDefectUnits { get; init; }
    public double YieldRate { get; init; }
}
