using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.DTOs.Alarm;

/// <summary>
/// Data transfer object for alarm list display.
/// </summary>
public record AlarmDto
{
    public Guid Id { get; init; }
    public string AlarmCode { get; init; } = string.Empty;
    public AlarmSeverity Severity { get; init; }
    public AlarmStatus Status { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public string EquipmentCode { get; init; } = string.Empty;
    public DateTime OccurredAt { get; init; }
    public DateTime? AcknowledgedAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? AcknowledgedBy { get; init; }
    public string? ResolvedBy { get; init; }
    public bool IsActive { get; init; }
    public bool IsAcknowledged { get; init; }
    public bool IsResolved { get; init; }
    public TimeSpan? TimeElapsed { get; init; }
}

/// <summary>
/// Detailed alarm information.
/// </summary>
public record AlarmDetailDto : AlarmDto
{
    public string? Description { get; init; }
    public string? ResolutionNotes { get; init; }
    public TimeSpan? TimeToAcknowledge { get; init; }
    public TimeSpan? TimeToResolve { get; init; }
    public string ProductionLineName { get; init; } = string.Empty;
}

/// <summary>
/// DTO for creating new alarm.
/// </summary>
public record AlarmCreateDto
{
    public Guid EquipmentId { get; init; }
    public string AlarmCode { get; init; } = string.Empty;
    public AlarmSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public string? Description { get; init; }
}

/// <summary>
/// DTO for acknowledging an alarm.
/// </summary>
public record AlarmAcknowledgeDto
{
    public string UserId { get; init; } = string.Empty;
}

/// <summary>
/// DTO for resolving an alarm.
/// </summary>
public record AlarmResolveDto
{
    public string UserId { get; init; } = string.Empty;
    public string? ResolutionNotes { get; init; }
}

/// <summary>
/// Filter criteria for alarm queries.
/// </summary>
public record AlarmFilterDto
{
    public Guid? FactoryId { get; init; }
    public Guid? EquipmentId { get; init; }
    public AlarmSeverity? Severity { get; init; }
    public AlarmStatus? Status { get; init; }
    public bool? ActiveOnly { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? SearchText { get; init; }
}

/// <summary>
/// Alarm summary for dashboard.
/// </summary>
public record AlarmSummaryDto
{
    public int TotalActive { get; init; }
    public int CriticalCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int InformationCount { get; init; }
    public int UnacknowledgedCount { get; init; }
    public int AcknowledgedCount { get; init; }
}
