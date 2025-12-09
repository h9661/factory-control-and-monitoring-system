using Microsoft.Extensions.Logging;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services.Simulation;

namespace SmartFactory.Application.Services.DataSource;

/// <summary>
/// Hybrid data source that attempts OPC-UA connection and falls back to simulation.
/// Provides seamless operation for demos that can optionally connect to real equipment.
/// </summary>
public class HybridDataSourceProvider : IDataSourceProvider
{
    private readonly ILogger<HybridDataSourceProvider> _logger;
    private readonly IDataSimulatorService _simulatorService;
    private readonly DataSourceOptions _options;

    private bool _isConnected;
    private bool _useSimulation = true;

    public DataSourceMode Mode => DataSourceMode.Hybrid;
    public bool IsConnected => _isConnected;
    public string StatusMessage { get; private set; } = "Disconnected";

    public event EventHandler<SimulatedSensorDataEventArgs>? SensorDataReceived;
    public event EventHandler<SimulatedEquipmentStatusEventArgs>? EquipmentStatusChanged;
    public event EventHandler<SimulatedAlarmEventArgs>? AlarmReceived;
    public event EventHandler<SimulatedProductionEventArgs>? ProductionReceived;
    public event EventHandler<DataSourceConnectionEventArgs>? ConnectionStatusChanged;

    public HybridDataSourceProvider(
        ILogger<HybridDataSourceProvider> logger,
        IDataSimulatorService simulatorService,
        DataSourceOptions options)
    {
        _logger = logger;
        _simulatorService = simulatorService;
        _options = options;

        // Wire up simulator events
        _simulatorService.SensorDataGenerated += OnSensorDataGenerated;
        _simulatorService.EquipmentStatusChanged += OnEquipmentStatusChanged;
        _simulatorService.AlarmGenerated += OnAlarmGenerated;
        _simulatorService.ProductionGenerated += OnProductionGenerated;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Hybrid data source...");

        // Try OPC-UA first if configured
        if (!string.IsNullOrEmpty(_options.OpcUaEndpoint))
        {
            try
            {
                // TODO: Implement OPC-UA connection attempt
                _logger.LogInformation("OPC-UA endpoint configured: {Endpoint}", _options.OpcUaEndpoint);
                // For now, fall through to simulation
                _useSimulation = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OPC-UA connection failed, falling back to simulation");
                _useSimulation = true;
            }
        }

        // Start simulation
        if (_useSimulation)
        {
            await _simulatorService.StartAsync(cancellationToken);
            _isConnected = true;
            StatusMessage = "Hybrid mode: Using simulation (OPC-UA not available)";

            ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
            {
                IsConnected = true,
                Mode = DataSourceMode.Hybrid,
                Message = "Running in simulation mode",
                Timestamp = DateTime.UtcNow
            });
        }

        _logger.LogInformation("Hybrid data source started (simulation={UseSimulation})", _useSimulation);
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Hybrid data source...");

        if (_useSimulation)
        {
            await _simulatorService.StopAsync(cancellationToken);
        }

        _isConnected = false;
        StatusMessage = "Stopped";

        ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
        {
            IsConnected = false,
            Mode = DataSourceMode.Hybrid,
            Message = "Hybrid data source stopped",
            Timestamp = DateTime.UtcNow
        });

        _logger.LogInformation("Hybrid data source stopped");
    }

    private void OnSensorDataGenerated(object? sender, SimulatedSensorDataEventArgs e)
    {
        if (_useSimulation)
            SensorDataReceived?.Invoke(this, e);
    }

    private void OnEquipmentStatusChanged(object? sender, SimulatedEquipmentStatusEventArgs e)
    {
        if (_useSimulation)
            EquipmentStatusChanged?.Invoke(this, e);
    }

    private void OnAlarmGenerated(object? sender, SimulatedAlarmEventArgs e)
    {
        if (_useSimulation)
            AlarmReceived?.Invoke(this, e);
    }

    private void OnProductionGenerated(object? sender, SimulatedProductionEventArgs e)
    {
        if (_useSimulation)
            ProductionReceived?.Invoke(this, e);
    }
}

/// <summary>
/// Configuration options for data source providers.
/// </summary>
public class DataSourceOptions
{
    public const string SectionName = "DataSource";

    /// <summary>
    /// The data source mode to use.
    /// </summary>
    public DataSourceMode Mode { get; set; } = DataSourceMode.Simulation;

    /// <summary>
    /// OPC-UA server endpoint URL (for OpcUa and Hybrid modes).
    /// </summary>
    public string? OpcUaEndpoint { get; set; }

    /// <summary>
    /// Simulation-specific options.
    /// </summary>
    public SimulationOptions Simulation { get; set; } = new();
}

/// <summary>
/// Simulation-specific configuration options.
/// </summary>
public class SimulationOptions
{
    /// <summary>
    /// Update interval for sensor data in milliseconds.
    /// </summary>
    public int UpdateIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Whether to use realistic patterns with trends and anomalies.
    /// </summary>
    public bool RealisticMode { get; set; } = true;

    /// <summary>
    /// Speed multiplier for simulation (1.0 = real-time).
    /// </summary>
    public double SpeedMultiplier { get; set; } = 1.0;
}
