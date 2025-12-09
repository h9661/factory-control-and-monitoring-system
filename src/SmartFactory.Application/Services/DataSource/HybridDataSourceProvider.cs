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
    private readonly IServiceProvider _serviceProvider;

    private IDataSourceProvider? _opcUaProvider;
    private bool _isConnected;
    private bool _useSimulation = true;
    private bool _useOpcUa;

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
        DataSourceOptions options,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _simulatorService = simulatorService;
        _options = options;
        _serviceProvider = serviceProvider;

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
                _logger.LogInformation("OPC-UA endpoint configured: {Endpoint}", _options.OpcUaEndpoint);

                // Try to resolve OpcUaDataSourceProvider from Infrastructure layer
                _opcUaProvider = ResolveOpcUaProvider();

                if (_opcUaProvider != null)
                {
                    // Wire up OPC-UA events
                    _opcUaProvider.SensorDataReceived += OnOpcUaSensorDataReceived;
                    _opcUaProvider.EquipmentStatusChanged += OnOpcUaEquipmentStatusChanged;
                    _opcUaProvider.AlarmReceived += OnOpcUaAlarmReceived;
                    _opcUaProvider.ProductionReceived += OnOpcUaProductionReceived;
                    _opcUaProvider.ConnectionStatusChanged += OnOpcUaConnectionStatusChanged;

                    // Attempt connection
                    await _opcUaProvider.StartAsync(cancellationToken);

                    if (_opcUaProvider.IsConnected)
                    {
                        _useOpcUa = true;
                        _useSimulation = false;
                        _isConnected = true;
                        StatusMessage = "Hybrid mode: Connected to OPC-UA";

                        _logger.LogInformation("OPC-UA connection successful, using real equipment data");

                        ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
                        {
                            IsConnected = true,
                            Mode = DataSourceMode.Hybrid,
                            Message = "Connected to OPC-UA servers",
                            Timestamp = DateTime.UtcNow
                        });

                        _logger.LogInformation("Hybrid data source started (using OPC-UA)");
                        return;
                    }
                }

                _logger.LogWarning("OPC-UA provider not available or failed to connect, falling back to simulation");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OPC-UA connection failed, falling back to simulation");
                CleanupOpcUaProvider();
            }
        }

        // Fall back to simulation
        _useSimulation = true;
        _useOpcUa = false;
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

        _logger.LogInformation("Hybrid data source started (using simulation)");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Hybrid data source...");

        if (_useOpcUa && _opcUaProvider != null)
        {
            await _opcUaProvider.StopAsync(cancellationToken);
            CleanupOpcUaProvider();
        }

        if (_useSimulation)
        {
            await _simulatorService.StopAsync(cancellationToken);
        }

        _isConnected = false;
        _useOpcUa = false;
        _useSimulation = false;
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

    private IDataSourceProvider? ResolveOpcUaProvider()
    {
        // Resolve OpcUaDataSourceProvider by type name to avoid direct reference to Infrastructure layer
        var opcUaProviderType = Type.GetType(
            "SmartFactory.Infrastructure.OpcUa.Services.OpcUaDataSourceProvider, SmartFactory.Infrastructure.OpcUa");

        if (opcUaProviderType != null)
        {
            return _serviceProvider.GetService(opcUaProviderType) as IDataSourceProvider;
        }

        _logger.LogDebug("OpcUaDataSourceProvider type not found. Ensure SmartFactory.Infrastructure.OpcUa is referenced.");
        return null;
    }

    private void CleanupOpcUaProvider()
    {
        if (_opcUaProvider != null)
        {
            _opcUaProvider.SensorDataReceived -= OnOpcUaSensorDataReceived;
            _opcUaProvider.EquipmentStatusChanged -= OnOpcUaEquipmentStatusChanged;
            _opcUaProvider.AlarmReceived -= OnOpcUaAlarmReceived;
            _opcUaProvider.ProductionReceived -= OnOpcUaProductionReceived;
            _opcUaProvider.ConnectionStatusChanged -= OnOpcUaConnectionStatusChanged;
            _opcUaProvider = null;
        }
    }

    // Simulation event handlers
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

    // OPC-UA event handlers
    private void OnOpcUaSensorDataReceived(object? sender, SimulatedSensorDataEventArgs e)
    {
        if (_useOpcUa)
            SensorDataReceived?.Invoke(this, e);
    }

    private void OnOpcUaEquipmentStatusChanged(object? sender, SimulatedEquipmentStatusEventArgs e)
    {
        if (_useOpcUa)
            EquipmentStatusChanged?.Invoke(this, e);
    }

    private void OnOpcUaAlarmReceived(object? sender, SimulatedAlarmEventArgs e)
    {
        if (_useOpcUa)
            AlarmReceived?.Invoke(this, e);
    }

    private void OnOpcUaProductionReceived(object? sender, SimulatedProductionEventArgs e)
    {
        if (_useOpcUa)
            ProductionReceived?.Invoke(this, e);
    }

    private async void OnOpcUaConnectionStatusChanged(object? sender, DataSourceConnectionEventArgs e)
    {
        if (!e.IsConnected && _useOpcUa)
        {
            _logger.LogWarning("OPC-UA connection lost, switching to simulation mode");

            _useOpcUa = false;
            _useSimulation = true;

            // Start simulation as fallback
            try
            {
                await _simulatorService.StartAsync();
                StatusMessage = "Hybrid mode: Using simulation (OPC-UA connection lost)";

                ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
                {
                    IsConnected = true,
                    Mode = DataSourceMode.Hybrid,
                    Message = "OPC-UA connection lost, switched to simulation",
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start simulation after OPC-UA connection loss");
                _isConnected = false;
                StatusMessage = "Disconnected - Both OPC-UA and simulation failed";
            }
        }
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
