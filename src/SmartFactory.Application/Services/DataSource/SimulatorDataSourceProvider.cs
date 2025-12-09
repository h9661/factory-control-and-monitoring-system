using Microsoft.Extensions.Logging;
using SmartFactory.Application.Interfaces;
using SmartFactory.Application.Services.Simulation;

namespace SmartFactory.Application.Services.DataSource;

/// <summary>
/// Data source provider that uses the simulation service.
/// Ideal for demos, testing, and development without real equipment.
/// </summary>
public class SimulatorDataSourceProvider : IDataSourceProvider
{
    private readonly ILogger<SimulatorDataSourceProvider> _logger;
    private readonly IDataSimulatorService _simulatorService;
    private bool _isConnected;

    public DataSourceMode Mode => DataSourceMode.Simulation;
    public bool IsConnected => _isConnected;
    public string StatusMessage { get; private set; } = "Disconnected";

    public event EventHandler<SimulatedSensorDataEventArgs>? SensorDataReceived;
    public event EventHandler<SimulatedEquipmentStatusEventArgs>? EquipmentStatusChanged;
    public event EventHandler<SimulatedAlarmEventArgs>? AlarmReceived;
    public event EventHandler<SimulatedProductionEventArgs>? ProductionReceived;
    public event EventHandler<DataSourceConnectionEventArgs>? ConnectionStatusChanged;

    public SimulatorDataSourceProvider(
        ILogger<SimulatorDataSourceProvider> logger,
        IDataSimulatorService simulatorService)
    {
        _logger = logger;
        _simulatorService = simulatorService;

        // Wire up simulator events to provider events
        _simulatorService.SensorDataGenerated += OnSensorDataGenerated;
        _simulatorService.EquipmentStatusChanged += OnEquipmentStatusChanged;
        _simulatorService.AlarmGenerated += OnAlarmGenerated;
        _simulatorService.ProductionGenerated += OnProductionGenerated;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting Simulator data source...");

        try
        {
            await _simulatorService.StartAsync(cancellationToken);
            _isConnected = true;
            StatusMessage = "Simulation running";

            ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
            {
                IsConnected = true,
                Mode = DataSourceMode.Simulation,
                Message = "Simulation started successfully",
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Simulator data source started");
        }
        catch (Exception ex)
        {
            _isConnected = false;
            StatusMessage = $"Failed to start: {ex.Message}";
            _logger.LogError(ex, "Failed to start simulator data source");
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Stopping Simulator data source...");

        try
        {
            await _simulatorService.StopAsync(cancellationToken);
            _isConnected = false;
            StatusMessage = "Simulation stopped";

            ConnectionStatusChanged?.Invoke(this, new DataSourceConnectionEventArgs
            {
                IsConnected = false,
                Mode = DataSourceMode.Simulation,
                Message = "Simulation stopped",
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Simulator data source stopped");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping simulator data source");
            throw;
        }
    }

    private void OnSensorDataGenerated(object? sender, SimulatedSensorDataEventArgs e)
    {
        SensorDataReceived?.Invoke(this, e);
    }

    private void OnEquipmentStatusChanged(object? sender, SimulatedEquipmentStatusEventArgs e)
    {
        EquipmentStatusChanged?.Invoke(this, e);
    }

    private void OnAlarmGenerated(object? sender, SimulatedAlarmEventArgs e)
    {
        AlarmReceived?.Invoke(this, e);
    }

    private void OnProductionGenerated(object? sender, SimulatedProductionEventArgs e)
    {
        ProductionReceived?.Invoke(this, e);
    }
}
