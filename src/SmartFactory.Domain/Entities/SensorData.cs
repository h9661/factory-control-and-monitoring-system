using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a sensor data reading from equipment.
/// This is a time-series entity optimized for high-volume inserts.
/// </summary>
public class SensorData
{
    public long Id { get; private set; }
    public Guid EquipmentId { get; private set; }
    public string TagName { get; private set; } = string.Empty;
    public double Value { get; private set; }
    public string? Unit { get; private set; }
    public DataQuality Quality { get; private set; }
    public DateTime Timestamp { get; private set; }

    public Equipment Equipment { get; private set; } = null!;

    // Required for EF Core
    private SensorData() { }

    public SensorData(Guid equipmentId, string tagName, double value, string? unit, DataQuality quality, DateTime timestamp)
    {
        EquipmentId = equipmentId;
        TagName = tagName;
        Value = value;
        Unit = unit;
        Quality = quality;
        Timestamp = timestamp;
    }

    public static SensorData Create(Guid equipmentId, string tagName, double value, string? unit = null)
    {
        return new SensorData(equipmentId, tagName, value, unit, DataQuality.Good, DateTime.UtcNow);
    }
}
