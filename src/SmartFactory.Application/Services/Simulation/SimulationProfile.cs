using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.Services.Simulation;

/// <summary>
/// Configuration profile for factory data simulation.
/// </summary>
public class SimulationProfile
{
    /// <summary>
    /// Update interval in milliseconds for sensor data generation.
    /// </summary>
    public int SensorUpdateIntervalMs { get; set; } = 2000;

    /// <summary>
    /// Update interval in milliseconds for equipment status checks.
    /// </summary>
    public int StatusUpdateIntervalMs { get; set; } = 5000;

    /// <summary>
    /// Update interval in milliseconds for production output.
    /// </summary>
    public int ProductionUpdateIntervalMs { get; set; } = 10000;

    /// <summary>
    /// Whether to generate realistic patterns with trends and anomalies.
    /// </summary>
    public bool RealisticMode { get; set; } = true;

    /// <summary>
    /// Base probability of generating an anomaly (0.0 to 1.0).
    /// </summary>
    public double AnomalyProbability { get; set; } = 0.05;

    /// <summary>
    /// Sensor configurations for each sensor type.
    /// </summary>
    public Dictionary<string, SensorSimulationConfig> SensorConfigs { get; set; } = new()
    {
        ["Temperature"] = new SensorSimulationConfig
        {
            BaseValue = 45.0,
            MinValue = 20.0,
            MaxValue = 100.0,
            NormalVariation = 5.0,
            AnomalyVariation = 25.0,
            Unit = "°C",
            WarningThreshold = 70.0,
            ErrorThreshold = 85.0
        },
        ["Vibration"] = new SensorSimulationConfig
        {
            BaseValue = 2.5,
            MinValue = 0.0,
            MaxValue = 15.0,
            NormalVariation = 0.5,
            AnomalyVariation = 5.0,
            Unit = "mm/s",
            WarningThreshold = 7.0,
            ErrorThreshold = 10.0
        },
        ["Pressure"] = new SensorSimulationConfig
        {
            BaseValue = 5.0,
            MinValue = 0.0,
            MaxValue = 10.0,
            NormalVariation = 0.3,
            AnomalyVariation = 2.0,
            Unit = "bar",
            WarningThreshold = 7.5,
            ErrorThreshold = 9.0
        },
        ["Current"] = new SensorSimulationConfig
        {
            BaseValue = 15.0,
            MinValue = 0.0,
            MaxValue = 50.0,
            NormalVariation = 2.0,
            AnomalyVariation = 15.0,
            Unit = "A",
            WarningThreshold = 35.0,
            ErrorThreshold = 45.0
        },
        ["Speed"] = new SensorSimulationConfig
        {
            BaseValue = 1200.0,
            MinValue = 0.0,
            MaxValue = 3000.0,
            NormalVariation = 50.0,
            AnomalyVariation = 500.0,
            Unit = "RPM",
            WarningThreshold = 2500.0,
            ErrorThreshold = 2800.0
        }
    };

    /// <summary>
    /// Status transition probability matrix.
    /// Key: Current status, Value: Dictionary of possible next statuses with probabilities.
    /// </summary>
    public Dictionary<EquipmentStatus, Dictionary<EquipmentStatus, double>> StatusTransitions { get; set; } = new()
    {
        [EquipmentStatus.Running] = new()
        {
            [EquipmentStatus.Running] = 0.90,
            [EquipmentStatus.Idle] = 0.05,
            [EquipmentStatus.Warning] = 0.03,
            [EquipmentStatus.Error] = 0.01,
            [EquipmentStatus.Maintenance] = 0.01
        },
        [EquipmentStatus.Idle] = new()
        {
            [EquipmentStatus.Idle] = 0.70,
            [EquipmentStatus.Running] = 0.25,
            [EquipmentStatus.Offline] = 0.03,
            [EquipmentStatus.Setup] = 0.02
        },
        [EquipmentStatus.Warning] = new()
        {
            [EquipmentStatus.Warning] = 0.40,
            [EquipmentStatus.Running] = 0.35,
            [EquipmentStatus.Error] = 0.15,
            [EquipmentStatus.Maintenance] = 0.10
        },
        [EquipmentStatus.Error] = new()
        {
            [EquipmentStatus.Error] = 0.30,
            [EquipmentStatus.Maintenance] = 0.50,
            [EquipmentStatus.Offline] = 0.15,
            [EquipmentStatus.Idle] = 0.05
        },
        [EquipmentStatus.Maintenance] = new()
        {
            [EquipmentStatus.Maintenance] = 0.60,
            [EquipmentStatus.Idle] = 0.30,
            [EquipmentStatus.Running] = 0.10
        },
        [EquipmentStatus.Offline] = new()
        {
            [EquipmentStatus.Offline] = 0.70,
            [EquipmentStatus.Idle] = 0.25,
            [EquipmentStatus.Setup] = 0.05
        },
        [EquipmentStatus.Setup] = new()
        {
            [EquipmentStatus.Setup] = 0.50,
            [EquipmentStatus.Running] = 0.35,
            [EquipmentStatus.Idle] = 0.15
        }
    };
}

/// <summary>
/// Configuration for simulating a specific sensor type.
/// </summary>
public class SensorSimulationConfig
{
    /// <summary>
    /// Base value around which normal readings fluctuate.
    /// </summary>
    public double BaseValue { get; set; }

    /// <summary>
    /// Minimum possible value for the sensor.
    /// </summary>
    public double MinValue { get; set; }

    /// <summary>
    /// Maximum possible value for the sensor.
    /// </summary>
    public double MaxValue { get; set; }

    /// <summary>
    /// Standard deviation for normal operating conditions.
    /// </summary>
    public double NormalVariation { get; set; }

    /// <summary>
    /// Additional variation applied during anomalies.
    /// </summary>
    public double AnomalyVariation { get; set; }

    /// <summary>
    /// Unit of measurement.
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Threshold value that triggers a warning.
    /// </summary>
    public double WarningThreshold { get; set; }

    /// <summary>
    /// Threshold value that triggers an error.
    /// </summary>
    public double ErrorThreshold { get; set; }
}
