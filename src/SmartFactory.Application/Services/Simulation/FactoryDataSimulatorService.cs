using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Services.Simulation;

/// <summary>
/// Service that generates realistic simulated factory data for demos and testing.
/// Produces sensor readings, equipment status changes, alarms, and production output.
/// </summary>
public class FactoryDataSimulatorService : IDataSimulatorService, IDisposable
{
    private readonly ILogger<FactoryDataSimulatorService> _logger;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly SimulationProfile _profile;
    private readonly Random _random = new();

    private Timer? _sensorTimer;
    private Timer? _statusTimer;
    private Timer? _productionTimer;
    private bool _isRunning;
    private bool _disposed;

    // Track current state for each equipment
    private readonly Dictionary<Guid, EquipmentSimulationState> _equipmentStates = new();

    public bool IsRunning => _isRunning;
    public double SpeedMultiplier { get; set; } = 1.0;

    public event EventHandler<SimulatedSensorDataEventArgs>? SensorDataGenerated;
    public event EventHandler<SimulatedEquipmentStatusEventArgs>? EquipmentStatusChanged;
    public event EventHandler<SimulatedAlarmEventArgs>? AlarmGenerated;
    public event EventHandler<SimulatedProductionEventArgs>? ProductionGenerated;

    public FactoryDataSimulatorService(
        ILogger<FactoryDataSimulatorService> logger,
        IEquipmentRepository equipmentRepository,
        IOptions<SimulationProfile> profileOptions)
    {
        _logger = logger;
        _equipmentRepository = equipmentRepository;
        _profile = profileOptions.Value;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Simulation is already running");
            return;
        }

        _logger.LogInformation("Starting factory data simulation...");

        // Initialize equipment states
        await InitializeEquipmentStatesAsync(cancellationToken);

        // Calculate intervals adjusted by speed multiplier
        var sensorInterval = (int)(_profile.SensorUpdateIntervalMs / SpeedMultiplier);
        var statusInterval = (int)(_profile.StatusUpdateIntervalMs / SpeedMultiplier);
        var productionInterval = (int)(_profile.ProductionUpdateIntervalMs / SpeedMultiplier);

        // Start timers
        _sensorTimer = new Timer(GenerateSensorData, null, 0, sensorInterval);
        _statusTimer = new Timer(UpdateEquipmentStatuses, null, 1000, statusInterval);
        _productionTimer = new Timer(GenerateProductionOutput, null, 2000, productionInterval);

        _isRunning = true;
        _logger.LogInformation("Factory data simulation started with {EquipmentCount} equipment units", _equipmentStates.Count);
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            return Task.CompletedTask;
        }

        _logger.LogInformation("Stopping factory data simulation...");

        _sensorTimer?.Dispose();
        _statusTimer?.Dispose();
        _productionTimer?.Dispose();

        _sensorTimer = null;
        _statusTimer = null;
        _productionTimer = null;

        _isRunning = false;
        _logger.LogInformation("Factory data simulation stopped");

        return Task.CompletedTask;
    }

    private async Task InitializeEquipmentStatesAsync(CancellationToken cancellationToken)
    {
        var equipment = await _equipmentRepository.GetAllAsync(cancellationToken);

        // Filter to active equipment only
        foreach (var eq in equipment.Where(e => e.IsActive))
        {
            _equipmentStates[eq.Id] = new EquipmentSimulationState
            {
                EquipmentId = eq.Id,
                EquipmentCode = eq.Code,
                EquipmentName = eq.Name,
                CurrentStatus = eq.Status,
                SensorValues = InitializeSensorValues(),
                LastStatusChange = DateTime.UtcNow,
                TrendDirection = _random.NextDouble() > 0.5 ? 1 : -1,
                CyclePhase = _random.NextDouble() * 2 * Math.PI
            };
        }
    }

    private Dictionary<string, double> InitializeSensorValues()
    {
        var values = new Dictionary<string, double>();
        foreach (var config in _profile.SensorConfigs)
        {
            // Start with base value plus small random variation
            values[config.Key] = config.Value.BaseValue + ((_random.NextDouble() - 0.5) * config.Value.NormalVariation);
        }
        return values;
    }

    private void GenerateSensorData(object? state)
    {
        if (!_isRunning) return;

        try
        {
            var timestamp = DateTime.UtcNow;

            foreach (var kvp in _equipmentStates)
            {
                var equipmentState = kvp.Value;

                // Skip offline equipment
                if (equipmentState.CurrentStatus == EquipmentStatus.Offline)
                    continue;

                foreach (var sensorConfig in _profile.SensorConfigs)
                {
                    var sensorName = sensorConfig.Key;
                    var config = sensorConfig.Value;

                    // Generate new sensor value
                    var newValue = GenerateSensorValue(equipmentState, sensorName, config);
                    equipmentState.SensorValues[sensorName] = newValue;

                    // Check for anomaly
                    var isAnomaly = newValue >= config.WarningThreshold;

                    // Raise event
                    SensorDataGenerated?.Invoke(this, new SimulatedSensorDataEventArgs
                    {
                        EquipmentId = equipmentState.EquipmentId,
                        EquipmentCode = equipmentState.EquipmentCode,
                        TagName = sensorName,
                        Value = Math.Round(newValue, 2),
                        Unit = config.Unit,
                        Timestamp = timestamp,
                        IsAnomaly = isAnomaly
                    });

                    // Generate alarm if threshold exceeded
                    if (isAnomaly && _random.NextDouble() < 0.3)
                    {
                        GenerateAlarmForSensor(equipmentState, sensorName, newValue, config, timestamp);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating sensor data");
        }
    }

    private double GenerateSensorValue(EquipmentSimulationState state, string sensorName, SensorSimulationConfig config)
    {
        var currentValue = state.SensorValues.GetValueOrDefault(sensorName, config.BaseValue);

        // Base target depends on equipment status
        var baseTarget = GetStatusAdjustedBaseValue(state.CurrentStatus, config);

        // Add trend component (slow drift)
        state.TrendDirection += (_random.NextDouble() - 0.5) * 0.1;
        state.TrendDirection = Math.Clamp(state.TrendDirection, -1, 1);

        // Add cyclical component (simulates operational cycles)
        state.CyclePhase += 0.1;
        var cyclicalComponent = Math.Sin(state.CyclePhase) * config.NormalVariation * 0.3;

        // Random noise
        var noise = (_random.NextDouble() - 0.5) * config.NormalVariation;

        // Anomaly injection
        var anomalyComponent = 0.0;
        if (_profile.RealisticMode && _random.NextDouble() < _profile.AnomalyProbability)
        {
            anomalyComponent = (_random.NextDouble() - 0.3) * config.AnomalyVariation;
            state.IsInAnomalyState = true;
        }
        else if (state.IsInAnomalyState && _random.NextDouble() < 0.2)
        {
            // Recovery from anomaly
            state.IsInAnomalyState = false;
        }

        // Smooth transition toward target
        var targetValue = baseTarget + cyclicalComponent + noise + anomalyComponent;
        var newValue = currentValue + (targetValue - currentValue) * 0.3;

        // Clamp to valid range
        return Math.Clamp(newValue, config.MinValue, config.MaxValue);
    }

    private double GetStatusAdjustedBaseValue(EquipmentStatus status, SensorSimulationConfig config)
    {
        return status switch
        {
            EquipmentStatus.Running => config.BaseValue * 1.1,
            EquipmentStatus.Idle => config.BaseValue * 0.6,
            EquipmentStatus.Warning => config.BaseValue * 1.3,
            EquipmentStatus.Error => config.BaseValue * 1.5,
            EquipmentStatus.Maintenance => config.BaseValue * 0.4,
            EquipmentStatus.Setup => config.BaseValue * 0.8,
            _ => config.BaseValue * 0.2
        };
    }

    private void GenerateAlarmForSensor(EquipmentSimulationState state, string sensorName, double value,
        SensorSimulationConfig config, DateTime timestamp)
    {
        var severity = value >= config.ErrorThreshold ? AlarmSeverity.Error :
                       value >= config.WarningThreshold ? AlarmSeverity.Warning :
                       AlarmSeverity.Information;

        var alarmCode = $"ALM_{sensorName.ToUpper()}_{(value >= config.ErrorThreshold ? "HIGH" : "WARN")}";
        var message = $"{sensorName} value {value:F1}{config.Unit} exceeds threshold ({config.WarningThreshold}{config.Unit})";

        AlarmGenerated?.Invoke(this, new SimulatedAlarmEventArgs
        {
            EquipmentId = state.EquipmentId,
            EquipmentCode = state.EquipmentCode,
            AlarmCode = alarmCode,
            Severity = severity,
            Message = message,
            Timestamp = timestamp
        });
    }

    private void UpdateEquipmentStatuses(object? state)
    {
        if (!_isRunning) return;

        try
        {
            var timestamp = DateTime.UtcNow;

            foreach (var kvp in _equipmentStates)
            {
                var equipmentState = kvp.Value;

                // Get transition probabilities for current status
                if (!_profile.StatusTransitions.TryGetValue(equipmentState.CurrentStatus, out var transitions))
                    continue;

                // Select next status based on probabilities
                var nextStatus = SelectNextStatus(transitions);

                if (nextStatus != equipmentState.CurrentStatus)
                {
                    var previousStatus = equipmentState.CurrentStatus;
                    equipmentState.CurrentStatus = nextStatus;
                    equipmentState.LastStatusChange = timestamp;

                    EquipmentStatusChanged?.Invoke(this, new SimulatedEquipmentStatusEventArgs
                    {
                        EquipmentId = equipmentState.EquipmentId,
                        EquipmentCode = equipmentState.EquipmentCode,
                        EquipmentName = equipmentState.EquipmentName,
                        PreviousStatus = previousStatus,
                        NewStatus = nextStatus,
                        Timestamp = timestamp
                    });

                    _logger.LogDebug("Equipment {Code} status changed: {Previous} -> {New}",
                        equipmentState.EquipmentCode, previousStatus, nextStatus);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating equipment statuses");
        }
    }

    private EquipmentStatus SelectNextStatus(Dictionary<EquipmentStatus, double> transitions)
    {
        var roll = _random.NextDouble();
        var cumulative = 0.0;

        foreach (var transition in transitions)
        {
            cumulative += transition.Value;
            if (roll <= cumulative)
            {
                return transition.Key;
            }
        }

        return transitions.Keys.First();
    }

    private void GenerateProductionOutput(object? state)
    {
        if (!_isRunning) return;

        try
        {
            var timestamp = DateTime.UtcNow;

            foreach (var kvp in _equipmentStates)
            {
                var equipmentState = kvp.Value;

                // Only running equipment produces output
                if (equipmentState.CurrentStatus != EquipmentStatus.Running)
                    continue;

                // Generate production units (varies by equipment)
                var baseUnits = _random.Next(5, 20);
                var defectRate = equipmentState.IsInAnomalyState ? 0.15 : 0.02;
                var defects = (int)(baseUnits * defectRate);

                ProductionGenerated?.Invoke(this, new SimulatedProductionEventArgs
                {
                    EquipmentId = equipmentState.EquipmentId,
                    EquipmentCode = equipmentState.EquipmentCode,
                    UnitsProduced = baseUnits,
                    DefectCount = defects,
                    Timestamp = timestamp
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating production output");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;

        StopAsync().GetAwaiter().GetResult();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Internal state tracking for simulated equipment.
/// </summary>
internal class EquipmentSimulationState
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public EquipmentStatus CurrentStatus { get; set; }
    public Dictionary<string, double> SensorValues { get; set; } = new();
    public DateTime LastStatusChange { get; set; }
    public double TrendDirection { get; set; }
    public double CyclePhase { get; set; }
    public bool IsInAnomalyState { get; set; }
}
