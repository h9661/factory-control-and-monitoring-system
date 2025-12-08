using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.DTOs.Maintenance;

/// <summary>
/// Data transfer object for maintenance record list display.
/// </summary>
public record MaintenanceRecordDto
{
    public Guid Id { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public string EquipmentCode { get; init; } = string.Empty;
    public MaintenanceType Type { get; init; }
    public MaintenanceStatus Status { get; init; }
    public string Title { get; init; } = string.Empty;
    public DateTime ScheduledDate { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public string? TechnicianName { get; init; }
    public decimal? EstimatedCost { get; init; }
    public decimal? ActualCost { get; init; }
    public int? DowntimeMinutes { get; init; }
    public bool IsOverdue { get; init; }
    public TimeSpan? Duration { get; init; }
}

/// <summary>
/// Detailed maintenance record information.
/// </summary>
public record MaintenanceRecordDetailDto : MaintenanceRecordDto
{
    public string? Description { get; init; }
    public string? TechnicianId { get; init; }
    public string? Notes { get; init; }
    public string? PartsUsed { get; init; }
    public string ProductionLineName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO for scheduling new maintenance.
/// </summary>
public record MaintenanceCreateDto
{
    public Guid EquipmentId { get; init; }
    public MaintenanceType Type { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public DateTime ScheduledDate { get; init; }
    public string? TechnicianId { get; init; }
    public string? TechnicianName { get; init; }
    public decimal? EstimatedCost { get; init; }
}

/// <summary>
/// DTO for completing maintenance.
/// </summary>
public record MaintenanceCompleteDto
{
    public decimal? ActualCost { get; init; }
    public int? DowntimeMinutes { get; init; }
    public string? Notes { get; init; }
    public string? PartsUsed { get; init; }
}

/// <summary>
/// DTO for rescheduling maintenance.
/// </summary>
public record MaintenanceRescheduleDto
{
    public DateTime NewScheduledDate { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// Filter criteria for maintenance queries.
/// </summary>
public record MaintenanceFilterDto
{
    public Guid? FactoryId { get; init; }
    public Guid? EquipmentId { get; init; }
    public MaintenanceType? Type { get; init; }
    public MaintenanceStatus? Status { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? TechnicianId { get; init; }
    public string? SearchText { get; init; }
}

/// <summary>
/// Alert for maintenance due.
/// </summary>
public record MaintenanceDueAlertDto
{
    public Guid? MaintenanceRecordId { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public string ProductionLineName { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public DateTime? ScheduledDate { get; init; }
    public DateTime? LastMaintenanceDate { get; init; }
    public DateTime DueDate { get; init; }
    public int DaysOverdue { get; init; }
    public string Severity { get; init; } = string.Empty;
    public bool IsOverdue { get; init; }
}

/// <summary>
/// Maintenance summary for reports.
/// </summary>
public record MaintenanceSummaryDto
{
    public int TotalScheduled { get; init; }
    public int TotalCompleted { get; init; }
    public int TotalCancelled { get; init; }
    public int TotalOverdue { get; init; }
    public int TotalInProgress { get; init; }
    public decimal TotalCost { get; init; }
    public decimal TotalActualCost { get; init; }
    public int PreventiveCount { get; init; }
    public int CorrectiveCount { get; init; }
    public int PredictiveCount { get; init; }
    public double CompletionRate { get; init; }
    public int TotalDowntimeMinutes { get; init; }
}
