using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a step/operation within a work order.
/// </summary>
public class WorkOrderStep : BaseEntity
{
    public Guid WorkOrderId { get; private set; }
    public Guid EquipmentId { get; private set; }
    public int Sequence { get; private set; }
    public WorkOrderStatus Status { get; private set; } = WorkOrderStatus.Scheduled;
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int TargetQuantity { get; private set; }
    public int ActualQuantity { get; private set; }
    public int DefectCount { get; private set; }
    public string? Notes { get; private set; }

    public WorkOrder WorkOrder { get; private set; } = null!;
    public Equipment Equipment { get; private set; } = null!;

    // Required for EF Core
    private WorkOrderStep() { }

    public WorkOrderStep(Guid workOrderId, Guid equipmentId, int sequence)
    {
        WorkOrderId = workOrderId;
        EquipmentId = equipmentId;
        Sequence = sequence;
    }

    public void SetTargetQuantity(int quantity)
    {
        TargetQuantity = quantity;
    }

    public void Start()
    {
        if (Status != WorkOrderStatus.Scheduled)
            throw new InvalidOperationException("Can only start scheduled steps.");

        Status = WorkOrderStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(int actualQuantity, int defectCount)
    {
        if (Status != WorkOrderStatus.InProgress)
            throw new InvalidOperationException("Can only complete in-progress steps.");

        Status = WorkOrderStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ActualQuantity = actualQuantity;
        DefectCount = defectCount;
    }

    public void UpdateProgress(int actualQuantity, int defectCount)
    {
        ActualQuantity = actualQuantity;
        DefectCount = defectCount;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
    }

    public TimeSpan? Duration => StartedAt.HasValue && CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;
}
