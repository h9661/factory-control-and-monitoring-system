using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a production work order.
/// </summary>
public class WorkOrder : BaseEntity
{
    public Guid FactoryId { get; private set; }
    public string OrderNumber { get; private set; } = string.Empty;
    public string ProductCode { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public int TargetQuantity { get; private set; }
    public int CompletedQuantity { get; private set; }
    public int DefectQuantity { get; private set; }
    public WorkOrderPriority Priority { get; private set; } = WorkOrderPriority.Normal;
    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Draft;
    public DateTime ScheduledStart { get; private set; }
    public DateTime ScheduledEnd { get; private set; }
    public DateTime? ActualStart { get; private set; }
    public DateTime? ActualEnd { get; private set; }
    public string? Notes { get; private set; }
    public string? CustomerName { get; private set; }
    public string? CustomerOrderRef { get; private set; }

    public Factory Factory { get; private set; } = null!;

    private readonly List<WorkOrderStep> _steps = new();
    public IReadOnlyCollection<WorkOrderStep> Steps => _steps.AsReadOnly();

    private readonly List<QualityRecord> _qualityRecords = new();
    public IReadOnlyCollection<QualityRecord> QualityRecords => _qualityRecords.AsReadOnly();

    // Required for EF Core
    private WorkOrder() { }

    public WorkOrder(
        Guid factoryId,
        string orderNumber,
        string productCode,
        string productName,
        int targetQuantity,
        DateTime scheduledStart,
        DateTime scheduledEnd)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(productCode);

        FactoryId = factoryId;
        OrderNumber = orderNumber;
        ProductCode = productCode;
        ProductName = productName;
        TargetQuantity = targetQuantity;
        ScheduledStart = scheduledStart;
        ScheduledEnd = scheduledEnd;
    }

    public void UpdateSchedule(DateTime start, DateTime end)
    {
        if (Status == WorkOrderStatus.Completed || Status == WorkOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot modify completed or cancelled work order.");

        ScheduledStart = start;
        ScheduledEnd = end;
    }

    public void SetPriority(WorkOrderPriority priority)
    {
        Priority = priority;
    }

    public void SetCustomerInfo(string? customerName, string? orderRef)
    {
        CustomerName = customerName;
        CustomerOrderRef = orderRef;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
    }

    public void Start()
    {
        if (Status != WorkOrderStatus.Draft && Status != WorkOrderStatus.Scheduled)
            throw new InvalidOperationException("Can only start draft or scheduled work orders.");

        Status = WorkOrderStatus.InProgress;
        ActualStart = DateTime.UtcNow;
    }

    public void Pause()
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Can only pause in-progress work orders.");

        Status = WorkOrderStatus.Paused;
    }

    public void Resume()
    {
        if (Status != WorkOrderStatus.Paused)
            throw new InvalidOperationException("Can only resume paused work orders.");

        Status = WorkOrderStatus.InProgress;
    }

    public void Complete()
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Can only complete in-progress work orders.");

        Status = WorkOrderStatus.Completed;
        ActualEnd = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == WorkOrderStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed work order.");

        Status = WorkOrderStatus.Cancelled;
        Notes = $"{Notes}\nCancellation reason: {reason}";
    }

    public void ReportProgress(int completedCount, int defectCount)
    {
        CompletedQuantity = completedCount;
        DefectQuantity = defectCount;
    }

    public WorkOrderStep AddStep(Guid equipmentId, int sequence)
    {
        var step = new WorkOrderStep(Id, equipmentId, sequence);
        _steps.Add(step);
        return step;
    }

    public double YieldRate => TargetQuantity > 0
        ? (double)(CompletedQuantity - DefectQuantity) / TargetQuantity * 100
        : 0;

    public double CompletionPercentage => TargetQuantity > 0
        ? (double)CompletedQuantity / TargetQuantity * 100
        : 0;

    public bool IsOnSchedule => Status switch
    {
        WorkOrderStatus.Completed => ActualEnd <= ScheduledEnd,
        WorkOrderStatus.InProgress => DateTime.UtcNow <= ScheduledEnd,
        _ => true
    };
}
