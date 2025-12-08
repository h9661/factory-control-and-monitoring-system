using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.Events;

/// <summary>
/// Event raised when equipment status changes.
/// </summary>
public record EquipmentStatusUpdatedEvent : DomainEvent
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public EquipmentStatus PreviousStatus { get; init; }
    public EquipmentStatus CurrentStatus { get; init; }
    public Guid? ProductionLineId { get; init; }
    public string? ProductionLineName { get; init; }
}

/// <summary>
/// Event raised when a new alarm is created.
/// </summary>
public record AlarmCreatedEvent : DomainEvent
{
    public Guid AlarmId { get; init; }
    public string AlarmCode { get; init; } = string.Empty;
    public AlarmSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public Guid? ProductionLineId { get; init; }
    public string? ProductionLineName { get; init; }
}

/// <summary>
/// Event raised when an alarm is acknowledged.
/// </summary>
public record AlarmAcknowledgedEvent : DomainEvent
{
    public Guid AlarmId { get; init; }
    public string AlarmCode { get; init; } = string.Empty;
    public AlarmSeverity Severity { get; init; }
    public string AcknowledgedBy { get; init; } = string.Empty;
    public DateTime AcknowledgedAt { get; init; }
}

/// <summary>
/// Event raised when an alarm is resolved.
/// </summary>
public record AlarmResolvedEvent : DomainEvent
{
    public Guid AlarmId { get; init; }
    public string AlarmCode { get; init; } = string.Empty;
    public AlarmSeverity Severity { get; init; }
    public string ResolvedBy { get; init; } = string.Empty;
    public DateTime ResolvedAt { get; init; }
    public string? ResolutionNotes { get; init; }
}

/// <summary>
/// Event raised when work order status changes.
/// </summary>
public record WorkOrderStatusChangedEvent : DomainEvent
{
    public Guid WorkOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public WorkOrderStatus PreviousStatus { get; init; }
    public WorkOrderStatus CurrentStatus { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public Guid FactoryId { get; init; }
    public string? FactoryName { get; init; }
}

/// <summary>
/// Event raised when work order progress is updated.
/// </summary>
public record WorkOrderProgressUpdatedEvent : DomainEvent
{
    public Guid WorkOrderId { get; init; }
    public string OrderNumber { get; init; } = string.Empty;
    public int CompletedQuantity { get; init; }
    public int TargetQuantity { get; init; }
    public int DefectQuantity { get; init; }
    public double ProgressPercentage { get; init; }
}

/// <summary>
/// Event raised when maintenance is due or overdue.
/// </summary>
public record MaintenanceDueEvent : DomainEvent
{
    public Guid? MaintenanceRecordId { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public MaintenanceType MaintenanceType { get; init; }
    public DateTime DueDate { get; init; }
    public int DaysOverdue { get; init; }
    public bool IsOverdue { get; init; }
}

/// <summary>
/// Event raised when maintenance is completed.
/// </summary>
public record MaintenanceCompletedEvent : DomainEvent
{
    public Guid MaintenanceRecordId { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public MaintenanceType MaintenanceType { get; init; }
    public TimeSpan Duration { get; init; }
    public decimal? ActualCost { get; init; }
    public int? DowntimeMinutes { get; init; }
}

/// <summary>
/// Event raised when sensor data is received.
/// </summary>
public record SensorDataReceivedEvent : DomainEvent
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string SensorType { get; init; } = string.Empty;
    public double Value { get; init; }
    public string? Unit { get; init; }
    public bool IsAnomaly { get; init; }
}

/// <summary>
/// Event raised when quality inspection is recorded.
/// </summary>
public record QualityInspectionRecordedEvent : DomainEvent
{
    public Guid QualityRecordId { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public InspectionType InspectionType { get; init; }
    public InspectionResult Result { get; init; }
    public DefectType? DefectType { get; init; }
    public int? DefectCount { get; init; }
}

/// <summary>
/// Event raised when production summary is updated.
/// </summary>
public record ProductionSummaryUpdatedEvent : DomainEvent
{
    public Guid? FactoryId { get; init; }
    public string? FactoryName { get; init; }
    public int TotalWorkOrders { get; init; }
    public int CompletedWorkOrders { get; init; }
    public int InProgressWorkOrders { get; init; }
    public int TotalTargetUnits { get; init; }
    public int TotalCompletedUnits { get; init; }
    public double OeeScore { get; init; }
}

/// <summary>
/// Event raised when alarm summary changes.
/// </summary>
public record AlarmSummaryChangedEvent : DomainEvent
{
    public Guid? FactoryId { get; init; }
    public int TotalActiveAlarms { get; init; }
    public int CriticalCount { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int UnacknowledgedCount { get; init; }
}
