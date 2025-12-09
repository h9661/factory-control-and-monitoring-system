using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.Services.Simulation;

/// <summary>
/// Interface for factory data simulation service.
/// Provides realistic simulated data for demo and testing purposes.
/// </summary>
public interface IDataSimulatorService
{
    /// <summary>
    /// Gets whether the simulation is currently running.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Gets or sets the simulation speed multiplier (1.0 = real-time).
    /// </summary>
    double SpeedMultiplier { get; set; }

    /// <summary>
    /// Starts the data simulation.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the data simulation.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when sensor data is generated.
    /// </summary>
    event EventHandler<SimulatedSensorDataEventArgs>? SensorDataGenerated;

    /// <summary>
    /// Event raised when equipment status changes.
    /// </summary>
    event EventHandler<SimulatedEquipmentStatusEventArgs>? EquipmentStatusChanged;

    /// <summary>
    /// Event raised when an alarm is generated.
    /// </summary>
    event EventHandler<SimulatedAlarmEventArgs>? AlarmGenerated;

    /// <summary>
    /// Event raised when production output is generated.
    /// </summary>
    event EventHandler<SimulatedProductionEventArgs>? ProductionGenerated;
}

/// <summary>
/// Event arguments for simulated sensor data.
/// </summary>
public class SimulatedSensorDataEventArgs : EventArgs
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string TagName { get; init; } = string.Empty;
    public double Value { get; init; }
    public string Unit { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
    public bool IsAnomaly { get; init; }
}

/// <summary>
/// Event arguments for simulated equipment status changes.
/// </summary>
public class SimulatedEquipmentStatusEventArgs : EventArgs
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public EquipmentStatus PreviousStatus { get; init; }
    public EquipmentStatus NewStatus { get; init; }
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Event arguments for simulated alarms.
/// </summary>
public class SimulatedAlarmEventArgs : EventArgs
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string AlarmCode { get; init; } = string.Empty;
    public AlarmSeverity Severity { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// Event arguments for simulated production output.
/// </summary>
public class SimulatedProductionEventArgs : EventArgs
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public int UnitsProduced { get; init; }
    public int DefectCount { get; init; }
    public DateTime Timestamp { get; init; }
}
