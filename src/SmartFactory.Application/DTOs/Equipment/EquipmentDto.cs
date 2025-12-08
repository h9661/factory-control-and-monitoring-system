using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.DTOs.Equipment;

/// <summary>
/// Data transfer object for equipment list display.
/// </summary>
public record EquipmentDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public EquipmentType Type { get; init; }
    public EquipmentStatus Status { get; init; }
    public Guid ProductionLineId { get; init; }
    public string ProductionLineName { get; init; } = string.Empty;
    public DateTime? LastHeartbeat { get; init; }
    public bool IsOnline { get; init; }
    public bool IsMaintenanceDue { get; init; }
    public bool IsActive { get; init; }
}

/// <summary>
/// Detailed equipment information including configuration.
/// </summary>
public record EquipmentDetailDto : EquipmentDto
{
    public string? OpcNodeId { get; init; }
    public string? IpAddress { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public DateTime? InstallationDate { get; init; }
    public DateTime? LastMaintenanceDate { get; init; }
    public int? MaintenanceIntervalDays { get; init; }
    public string? Description { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO for creating new equipment.
/// </summary>
public record EquipmentCreateDto
{
    public Guid ProductionLineId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public EquipmentType Type { get; init; }
    public string? Description { get; init; }
    public string? OpcNodeId { get; init; }
    public string? IpAddress { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public DateTime? InstallationDate { get; init; }
    public int? MaintenanceIntervalDays { get; init; }
}

/// <summary>
/// DTO for updating equipment.
/// </summary>
public record EquipmentUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public EquipmentType Type { get; init; }
    public bool IsActive { get; init; }
    public string? Description { get; init; }
    public string? OpcNodeId { get; init; }
    public string? IpAddress { get; init; }
    public string? Manufacturer { get; init; }
    public string? Model { get; init; }
    public string? SerialNumber { get; init; }
    public DateTime? InstallationDate { get; init; }
    public int? MaintenanceIntervalDays { get; init; }
}

/// <summary>
/// Filter criteria for equipment queries.
/// </summary>
public record EquipmentFilterDto
{
    public Guid? FactoryId { get; init; }
    public Guid? ProductionLineId { get; init; }
    public EquipmentStatus? Status { get; init; }
    public EquipmentType? Type { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsMaintenanceDue { get; init; }
    public string? SearchText { get; init; }
}

/// <summary>
/// Equipment status summary for dashboard.
/// </summary>
public record EquipmentStatusSummaryDto
{
    public int TotalCount { get; init; }
    public int RunningCount { get; init; }
    public int IdleCount { get; init; }
    public int WarningCount { get; init; }
    public int ErrorCount { get; init; }
    public int MaintenanceCount { get; init; }
    public int OfflineCount { get; init; }
    public int SetupCount { get; init; }
}
