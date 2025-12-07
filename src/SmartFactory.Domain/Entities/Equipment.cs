using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a piece of manufacturing equipment.
/// </summary>
public class Equipment : BaseEntity
{
    public Guid ProductionLineId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public EquipmentType Type { get; private set; }
    public EquipmentStatus Status { get; private set; } = EquipmentStatus.Offline;
    public string? OpcNodeId { get; private set; }
    public string? IpAddress { get; private set; }
    public string? Manufacturer { get; private set; }
    public string? Model { get; private set; }
    public string? SerialNumber { get; private set; }
    public DateTime? InstallationDate { get; private set; }
    public DateTime? LastHeartbeat { get; private set; }
    public DateTime? LastMaintenanceDate { get; private set; }
    public int? MaintenanceIntervalDays { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ProductionLine ProductionLine { get; private set; } = null!;

    private readonly List<SensorData> _sensorData = new();
    public IReadOnlyCollection<SensorData> SensorData => _sensorData.AsReadOnly();

    private readonly List<Alarm> _alarms = new();
    public IReadOnlyCollection<Alarm> Alarms => _alarms.AsReadOnly();

    private readonly List<MaintenanceRecord> _maintenanceRecords = new();
    public IReadOnlyCollection<MaintenanceRecord> MaintenanceRecords => _maintenanceRecords.AsReadOnly();

    // Required for EF Core
    private Equipment() { }

    public Equipment(Guid productionLineId, string code, string name, EquipmentType type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        ProductionLineId = productionLineId;
        Code = code.ToUpperInvariant();
        Name = name;
        Type = type;
    }

    public void Update(string name, string? description, EquipmentType type)
    {
        Name = name;
        Description = description;
        Type = type;
    }

    public void SetOpcConfiguration(string? nodeId)
    {
        OpcNodeId = nodeId;
    }

    public void SetNetworkConfiguration(string? ipAddress)
    {
        IpAddress = ipAddress;
    }

    public void SetManufacturerInfo(string? manufacturer, string? model, string? serialNumber)
    {
        Manufacturer = manufacturer;
        Model = model;
        SerialNumber = serialNumber;
    }

    public void SetInstallationDate(DateTime? date)
    {
        InstallationDate = date;
    }

    public void SetMaintenanceSchedule(int? intervalDays)
    {
        MaintenanceIntervalDays = intervalDays;
    }

    public void UpdateStatus(EquipmentStatus newStatus)
    {
        if (Status != newStatus)
        {
            Status = newStatus;
            LastHeartbeat = DateTime.UtcNow;
        }
    }

    public void RecordHeartbeat()
    {
        LastHeartbeat = DateTime.UtcNow;
    }

    public void RecordMaintenance()
    {
        LastMaintenanceDate = DateTime.UtcNow;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public bool IsMaintenanceDue()
    {
        if (!MaintenanceIntervalDays.HasValue || !LastMaintenanceDate.HasValue)
            return false;

        return DateTime.UtcNow > LastMaintenanceDate.Value.AddDays(MaintenanceIntervalDays.Value);
    }

    public bool IsOnline => Status != EquipmentStatus.Offline;
    public bool IsOperational => Status == EquipmentStatus.Running || Status == EquipmentStatus.Idle;
}
