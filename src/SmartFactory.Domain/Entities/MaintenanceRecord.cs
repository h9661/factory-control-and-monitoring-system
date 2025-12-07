using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a maintenance record for equipment.
/// </summary>
public class MaintenanceRecord : BaseEntity
{
    public Guid EquipmentId { get; private set; }
    public MaintenanceType Type { get; private set; }
    public MaintenanceStatus Status { get; private set; } = MaintenanceStatus.Scheduled;
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime ScheduledDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public string? TechnicianId { get; private set; }
    public string? TechnicianName { get; private set; }
    public string? Notes { get; private set; }
    public decimal? EstimatedCost { get; private set; }
    public decimal? ActualCost { get; private set; }
    public int? DowntimeMinutes { get; private set; }
    public string? PartsUsed { get; private set; }

    public Equipment Equipment { get; private set; } = null!;

    // Required for EF Core
    private MaintenanceRecord() { }

    public MaintenanceRecord(
        Guid equipmentId,
        MaintenanceType type,
        string title,
        DateTime scheduledDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);

        EquipmentId = equipmentId;
        Type = type;
        Title = title;
        ScheduledDate = scheduledDate;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    public void AssignTechnician(string technicianId, string? technicianName)
    {
        TechnicianId = technicianId;
        TechnicianName = technicianName;
    }

    public void SetEstimatedCost(decimal cost)
    {
        EstimatedCost = cost;
    }

    public void Reschedule(DateTime newDate)
    {
        if (Status == MaintenanceStatus.Completed || Status == MaintenanceStatus.Cancelled)
            throw new InvalidOperationException("Cannot reschedule completed or cancelled maintenance.");

        ScheduledDate = newDate;
    }

    public void Start()
    {
        if (Status != MaintenanceStatus.Scheduled && Status != MaintenanceStatus.Overdue)
            throw new InvalidOperationException("Can only start scheduled or overdue maintenance.");

        Status = MaintenanceStatus.InProgress;
        StartedAt = DateTime.UtcNow;
    }

    public void Complete(decimal? actualCost, int? downtimeMinutes, string? notes)
    {
        if (Status != MaintenanceStatus.InProgress)
            throw new InvalidOperationException("Can only complete in-progress maintenance.");

        Status = MaintenanceStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        ActualCost = actualCost;
        DowntimeMinutes = downtimeMinutes;
        Notes = notes;
    }

    public void Cancel(string reason)
    {
        if (Status == MaintenanceStatus.Completed)
            throw new InvalidOperationException("Cannot cancel completed maintenance.");

        Status = MaintenanceStatus.Cancelled;
        Notes = $"Cancelled: {reason}";
    }

    public void MarkOverdue()
    {
        if (Status == MaintenanceStatus.Scheduled && DateTime.UtcNow > ScheduledDate)
        {
            Status = MaintenanceStatus.Overdue;
        }
    }

    public void RecordPartsUsed(string parts)
    {
        PartsUsed = parts;
    }

    public TimeSpan? Duration => StartedAt.HasValue && CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : null;

    public bool IsOverdue => Status == MaintenanceStatus.Scheduled && DateTime.UtcNow > ScheduledDate;
}
