using SmartFactory.Application.Services.Simulation;

namespace SmartFactory.Application.Interfaces;

/// <summary>
/// Abstraction for factory data sources (Simulation, OPC-UA, or Hybrid).
/// Provides a unified interface for subscribing to real-time factory data.
/// </summary>
public interface IDataSourceProvider
{
    /// <summary>
    /// Gets the current data source mode.
    /// </summary>
    DataSourceMode Mode { get; }

    /// <summary>
    /// Gets whether the data source is currently connected and providing data.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Gets the connection status message.
    /// </summary>
    string StatusMessage { get; }

    /// <summary>
    /// Starts the data source and begins generating/receiving data.
    /// </summary>
    Task StartAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops the data source.
    /// </summary>
    Task StopAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Event raised when sensor data is received.
    /// </summary>
    event EventHandler<SimulatedSensorDataEventArgs>? SensorDataReceived;

    /// <summary>
    /// Event raised when equipment status changes.
    /// </summary>
    event EventHandler<SimulatedEquipmentStatusEventArgs>? EquipmentStatusChanged;

    /// <summary>
    /// Event raised when an alarm is generated.
    /// </summary>
    event EventHandler<SimulatedAlarmEventArgs>? AlarmReceived;

    /// <summary>
    /// Event raised when production output is generated.
    /// </summary>
    event EventHandler<SimulatedProductionEventArgs>? ProductionReceived;

    /// <summary>
    /// Event raised when connection status changes.
    /// </summary>
    event EventHandler<DataSourceConnectionEventArgs>? ConnectionStatusChanged;
}

/// <summary>
/// Data source operation mode.
/// </summary>
public enum DataSourceMode
{
    /// <summary>
    /// Simulated data for demos and testing.
    /// </summary>
    Simulation,

    /// <summary>
    /// Real OPC-UA connection to industrial equipment.
    /// </summary>
    OpcUa,

    /// <summary>
    /// Hybrid mode: OPC-UA when available, simulation as fallback.
    /// </summary>
    Hybrid
}

/// <summary>
/// Event arguments for connection status changes.
/// </summary>
public class DataSourceConnectionEventArgs : EventArgs
{
    public bool IsConnected { get; init; }
    public DataSourceMode Mode { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime Timestamp { get; init; }
}
