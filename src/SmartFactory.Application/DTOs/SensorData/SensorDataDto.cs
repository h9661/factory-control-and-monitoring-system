using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.DTOs.SensorData;

/// <summary>
/// Data transfer object for sensor data.
/// </summary>
public record SensorDataDto
{
    public long Id { get; init; }
    public Guid EquipmentId { get; init; }
    public string TagName { get; init; } = string.Empty;
    public double Value { get; init; }
    public string? Unit { get; init; }
    public DataQuality Quality { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// DTO for recording sensor data.
/// </summary>
public record SensorDataCreateDto
{
    public Guid EquipmentId { get; init; }
    public string TagName { get; init; } = string.Empty;
    public double Value { get; init; }
    public string? Unit { get; init; }
    public DataQuality Quality { get; init; } = DataQuality.Good;
    public DateTime? Timestamp { get; init; }
}

/// <summary>
/// Batch sensor data for bulk insert.
/// </summary>
public record SensorDataBatchDto
{
    public IEnumerable<SensorDataCreateDto> Data { get; init; } = Enumerable.Empty<SensorDataCreateDto>();
}

/// <summary>
/// Filter for sensor data queries.
/// </summary>
public record SensorDataFilterDto
{
    public Guid EquipmentId { get; init; }
    public string? TagName { get; init; }
    public DateTime DateFrom { get; init; }
    public DateTime DateTo { get; init; }
    public DataQuality? Quality { get; init; }
}

/// <summary>
/// Aggregated sensor data.
/// </summary>
public record SensorDataAggregateDto
{
    public string TagName { get; init; } = string.Empty;
    public double MinValue { get; init; }
    public double MaxValue { get; init; }
    public double AverageValue { get; init; }
    public int DataPointCount { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
}

/// <summary>
/// Latest sensor readings for equipment.
/// </summary>
public record EquipmentSensorReadingsDto
{
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public IEnumerable<LatestSensorValueDto> Readings { get; init; } = Enumerable.Empty<LatestSensorValueDto>();
}

/// <summary>
/// Latest sensor value.
/// </summary>
public record LatestSensorValueDto
{
    public string TagName { get; init; } = string.Empty;
    public double Value { get; init; }
    public string? Unit { get; init; }
    public DataQuality Quality { get; init; }
    public DateTime Timestamp { get; init; }
}
