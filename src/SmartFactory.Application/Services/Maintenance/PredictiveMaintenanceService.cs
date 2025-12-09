using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Maintenance;
using SmartFactory.Domain.Entities;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Services.Maintenance;

/// <summary>
/// Service for predictive maintenance calculations using weighted scoring algorithms.
/// </summary>
public class PredictiveMaintenanceService : IPredictiveMaintenanceService
{
    private readonly ILogger<PredictiveMaintenanceService> _logger;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly ISensorDataRepository _sensorDataRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;

    // Weighted scoring factors
    private const double WeightDaysSinceMaintenance = 0.25;
    private const double WeightSensorTrends = 0.35;
    private const double WeightOperatingHours = 0.20;
    private const double WeightHistoricalFailures = 0.20;

    // Thresholds
    private const double TemperatureWarningThreshold = 75.0;
    private const double TemperatureCriticalThreshold = 85.0;
    private const double VibrationWarningThreshold = 8.0;
    private const double VibrationCriticalThreshold = 12.0;
    private const double PressureWarningThreshold = 110.0;
    private const double PressureCriticalThreshold = 130.0;

    private readonly Random _random = new();

    public PredictiveMaintenanceService(
        ILogger<PredictiveMaintenanceService> logger,
        IEquipmentRepository equipmentRepository,
        ISensorDataRepository sensorDataRepository,
        IMaintenanceRepository maintenanceRepository)
    {
        _logger = logger;
        _equipmentRepository = equipmentRepository;
        _sensorDataRepository = sensorDataRepository;
        _maintenanceRepository = maintenanceRepository;
    }

    public async Task<HealthScoreDto> GetHealthScoreAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment == null)
        {
            return CreateDefaultHealthScore(equipmentId, "Unknown");
        }

        return await CalculateHealthScoreAsync(equipment, cancellationToken);
    }

    public async Task<List<HealthScoreDto>> GetFactoryHealthScoresAsync(
        Guid factoryId,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByFactoryAsync(factoryId, cancellationToken);
        var scores = new List<HealthScoreDto>();

        foreach (var eq in equipment.Where(e => e.IsActive))
        {
            var score = await CalculateHealthScoreAsync(eq, cancellationToken);
            scores.Add(score);
        }

        return scores.OrderBy(s => s.OverallScore).ToList();
    }

    public async Task<MaintenancePredictionDto> PredictMaintenanceAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        var healthScore = await GetHealthScoreAsync(equipmentId, cancellationToken);

        var daysUntilMaintenance = CalculateDaysUntilMaintenance(healthScore);
        var urgency = DetermineUrgency(daysUntilMaintenance, healthScore.RiskLevel);

        var warningIndicators = new List<string>();
        foreach (var concern in healthScore.TopConcerns)
        {
            warningIndicators.Add(concern);
        }

        return new MaintenancePredictionDto
        {
            EquipmentId = equipmentId,
            EquipmentName = healthScore.EquipmentName,
            PredictedMaintenanceDate = DateTime.UtcNow.AddDays(daysUntilMaintenance),
            DaysUntilMaintenance = daysUntilMaintenance,
            ConfidenceScore = CalculateConfidenceScore(healthScore),
            Urgency = urgency,
            RecommendedAction = GetRecommendedAction(urgency, healthScore.TopConcerns),
            WarningIndicators = warningIndicators,
            EstimatedDowntimeHours = EstimateDowntimeHours(urgency)
        };
    }

    public async Task<List<MaintenancePredictionDto>> GetFactoryMaintenancePredictionsAsync(
        Guid factoryId,
        CancellationToken cancellationToken = default)
    {
        var healthScores = await GetFactoryHealthScoresAsync(factoryId, cancellationToken);
        var predictions = new List<MaintenancePredictionDto>();

        foreach (var score in healthScores)
        {
            var prediction = await PredictMaintenanceAsync(score.EquipmentId, cancellationToken);
            predictions.Add(prediction);
        }

        return predictions.OrderBy(p => p.DaysUntilMaintenance).ToList();
    }

    public async Task<List<AnomalyResultDto>> DetectAnomaliesAsync(
        Guid equipmentId,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment == null) return new List<AnomalyResultDto>();

        var anomalies = new List<AnomalyResultDto>();

        // Simulate sensor data analysis for anomaly detection
        var sensorTypes = new[] { "Temperature", "Vibration", "Pressure" };

        foreach (var sensorType in sensorTypes)
        {
            var (currentValue, expectedValue) = GenerateSimulatedSensorValues(sensorType, equipment.Status);

            var deviation = Math.Abs((currentValue - expectedValue) / expectedValue) * 100;

            if (deviation > 15) // 15% deviation threshold
            {
                var severity = deviation switch
                {
                    > 50 => AnomalySeverity.Severe,
                    > 35 => AnomalySeverity.Significant,
                    > 25 => AnomalySeverity.Moderate,
                    _ => AnomalySeverity.Minor
                };

                anomalies.Add(new AnomalyResultDto
                {
                    EquipmentId = equipmentId,
                    EquipmentName = equipment.Name,
                    SensorName = sensorType,
                    CurrentValue = Math.Round(currentValue, 2),
                    ExpectedValue = Math.Round(expectedValue, 2),
                    DeviationPercent = Math.Round(deviation, 2),
                    Severity = severity,
                    DetectedAt = DateTime.UtcNow,
                    Description = $"{sensorType} reading is {deviation:F1}% above expected baseline",
                    IsActive = true
                });
            }
        }

        return anomalies;
    }

    public async Task<List<AnomalyResultDto>> GetActiveAnomaliesAsync(
        Guid factoryId,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByFactoryAsync(factoryId, cancellationToken);
        var allAnomalies = new List<AnomalyResultDto>();

        foreach (var eq in equipment.Where(e => e.IsActive))
        {
            var anomalies = await DetectAnomaliesAsync(eq.Id, cancellationToken);
            allAnomalies.AddRange(anomalies);
        }

        return allAnomalies.OrderByDescending(a => a.Severity).ToList();
    }

    public async Task<List<HealthTrendPointDto>> GetHealthTrendAsync(
        Guid equipmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var trendPoints = new List<HealthTrendPointDto>();
        var current = startDate;
        var interval = TimeSpan.FromHours(4);

        var baseHealthScore = 75.0 + (_random.NextDouble() - 0.5) * 20;
        var trend = -0.1; // Slight decline over time

        while (current < endDate)
        {
            // Add some variation with a general trend
            var variation = (_random.NextDouble() - 0.5) * 10;
            var healthScore = Math.Clamp(baseHealthScore + variation, 20, 100);

            trendPoints.Add(new HealthTrendPointDto
            {
                Timestamp = current,
                HealthScore = Math.Round(healthScore, 2),
                Temperature = Math.Round(45 + _random.NextDouble() * 30, 2),
                Vibration = Math.Round(3 + _random.NextDouble() * 6, 2),
                Pressure = Math.Round(90 + _random.NextDouble() * 30, 2)
            });

            baseHealthScore += trend + (_random.NextDouble() - 0.5) * 2;
            baseHealthScore = Math.Clamp(baseHealthScore, 30, 95);
            current += interval;
        }

        return trendPoints;
    }

    public async Task<List<HealthScoreDto>> GetRiskRankingAsync(
        Guid factoryId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var healthScores = await GetFactoryHealthScoresAsync(factoryId, cancellationToken);
        return healthScores.Take(limit).ToList();
    }

    private async Task<HealthScoreDto> CalculateHealthScoreAsync(
        Equipment equipment,
        CancellationToken cancellationToken)
    {
        var concerns = new List<string>();
        var componentScores = new List<ComponentHealthDto>();

        // Calculate days since maintenance score
        var daysSinceMaintenance = equipment.LastMaintenanceDate.HasValue
            ? (DateTime.UtcNow - equipment.LastMaintenanceDate.Value).TotalDays
            : 365;

        var maintenanceScore = Math.Max(0, 100 - (daysSinceMaintenance / 3));

        if (daysSinceMaintenance > 90)
            concerns.Add($"No maintenance in {(int)daysSinceMaintenance} days");

        // Simulate sensor-based scores
        var temperatureScore = CalculateSensorScore("Temperature", equipment.Status);
        var vibrationScore = CalculateSensorScore("Vibration", equipment.Status);
        var pressureScore = CalculateSensorScore("Pressure", equipment.Status);

        componentScores.Add(new ComponentHealthDto
        {
            ComponentName = "Temperature",
            Score = temperatureScore.score,
            Status = GetStatusFromScore(temperatureScore.score),
            TrendValue = temperatureScore.trend,
            Issue = temperatureScore.score < 60 ? "Elevated temperature detected" : null
        });

        componentScores.Add(new ComponentHealthDto
        {
            ComponentName = "Vibration",
            Score = vibrationScore.score,
            Status = GetStatusFromScore(vibrationScore.score),
            TrendValue = vibrationScore.trend,
            Issue = vibrationScore.score < 60 ? "Abnormal vibration patterns" : null
        });

        componentScores.Add(new ComponentHealthDto
        {
            ComponentName = "Pressure",
            Score = pressureScore.score,
            Status = GetStatusFromScore(pressureScore.score),
            TrendValue = pressureScore.trend,
            Issue = pressureScore.score < 60 ? "Pressure anomaly detected" : null
        });

        // Calculate operating hours score (simulated)
        var operatingHours = _random.Next(100, 2000);
        var operatingHoursScore = Math.Max(0, 100 - (operatingHours / 20.0));

        if (operatingHours > 1500)
            concerns.Add($"High operating hours: {operatingHours}h since maintenance");

        // Historical failure score (simulated)
        var failureScore = 70 + _random.NextDouble() * 30;

        // Weighted overall score
        var sensorAverageScore = (temperatureScore.score + vibrationScore.score + pressureScore.score) / 3.0;
        var overallScore =
            (maintenanceScore * WeightDaysSinceMaintenance) +
            (sensorAverageScore * WeightSensorTrends) +
            (operatingHoursScore * WeightOperatingHours) +
            (failureScore * WeightHistoricalFailures);

        // Add concerns based on component scores
        foreach (var component in componentScores.Where(c => c.Score < 60))
        {
            if (!string.IsNullOrEmpty(component.Issue) && !concerns.Contains(component.Issue))
                concerns.Add(component.Issue);
        }

        // Determine trend
        var trend = DetermineTrend(componentScores);

        return new HealthScoreDto
        {
            EquipmentId = equipment.Id,
            EquipmentName = equipment.Name,
            EquipmentCode = equipment.Code,
            OverallScore = Math.Round(overallScore, 2),
            ComponentScores = componentScores,
            DaysUntilMaintenanceRecommended = CalculateDaysUntilMaintenance(overallScore),
            TopConcerns = concerns,
            Trend = trend,
            LastMaintenanceDate = equipment.LastMaintenanceDate,
            OperatingHoursSinceLastMaintenance = operatingHours,
            CalculatedAt = DateTime.UtcNow
        };
    }

    private (double score, double trend) CalculateSensorScore(string sensorType, EquipmentStatus status)
    {
        var baseScore = status switch
        {
            EquipmentStatus.Running => 75 + _random.NextDouble() * 20,
            EquipmentStatus.Idle => 85 + _random.NextDouble() * 15,
            EquipmentStatus.Warning => 45 + _random.NextDouble() * 25,
            EquipmentStatus.Error => 20 + _random.NextDouble() * 30,
            EquipmentStatus.Maintenance => 50 + _random.NextDouble() * 20,
            _ => 60 + _random.NextDouble() * 30
        };

        var trend = (_random.NextDouble() - 0.5) * 10;
        return (Math.Round(baseScore, 2), Math.Round(trend, 2));
    }

    private (double currentValue, double expectedValue) GenerateSimulatedSensorValues(
        string sensorType,
        EquipmentStatus status)
    {
        return sensorType switch
        {
            "Temperature" => (
                45 + _random.NextDouble() * 40 * (status == EquipmentStatus.Warning ? 1.3 : 1.0),
                55
            ),
            "Vibration" => (
                3 + _random.NextDouble() * 8 * (status == EquipmentStatus.Error ? 1.5 : 1.0),
                5
            ),
            "Pressure" => (
                90 + _random.NextDouble() * 40 * (status == EquipmentStatus.Warning ? 1.2 : 1.0),
                100
            ),
            _ => (50, 50)
        };
    }

    private static string GetStatusFromScore(double score)
    {
        return score switch
        {
            >= 80 => "Excellent",
            >= 60 => "Good",
            >= 40 => "Fair",
            >= 20 => "Poor",
            _ => "Critical"
        };
    }

    private static HealthTrend DetermineTrend(List<ComponentHealthDto> components)
    {
        var avgTrend = components.Average(c => c.TrendValue);
        return avgTrend switch
        {
            > 5 => HealthTrend.Improving,
            > -2 => HealthTrend.Stable,
            > -10 => HealthTrend.Declining,
            _ => HealthTrend.RapidlyDeclining
        };
    }

    private static int CalculateDaysUntilMaintenance(HealthScoreDto healthScore)
    {
        return CalculateDaysUntilMaintenance(healthScore.OverallScore);
    }

    private static int CalculateDaysUntilMaintenance(double overallScore)
    {
        return overallScore switch
        {
            >= 80 => 90,
            >= 60 => 30,
            >= 40 => 14,
            >= 20 => 7,
            _ => 1
        };
    }

    private static MaintenanceUrgency DetermineUrgency(int daysUntil, RiskLevel riskLevel)
    {
        if (riskLevel == RiskLevel.Critical || daysUntil <= 1)
            return MaintenanceUrgency.Critical;
        if (riskLevel == RiskLevel.High || daysUntil <= 7)
            return MaintenanceUrgency.Urgent;
        if (daysUntil <= 14)
            return MaintenanceUrgency.Required;
        if (daysUntil <= 30)
            return MaintenanceUrgency.Scheduled;
        return MaintenanceUrgency.Routine;
    }

    private static double CalculateConfidenceScore(HealthScoreDto healthScore)
    {
        // Higher confidence with more data points and clearer trends
        var baseConfidence = 0.7;
        if (healthScore.ComponentScores.Count >= 3) baseConfidence += 0.1;
        if (healthScore.LastMaintenanceDate.HasValue) baseConfidence += 0.1;
        if (healthScore.Trend != HealthTrend.Stable) baseConfidence += 0.05;
        return Math.Min(baseConfidence, 0.95);
    }

    private static string GetRecommendedAction(MaintenanceUrgency urgency, List<string> concerns)
    {
        var primaryConcern = concerns.FirstOrDefault() ?? "General maintenance";

        return urgency switch
        {
            MaintenanceUrgency.Critical => $"IMMEDIATE ACTION: Stop equipment and perform emergency maintenance. {primaryConcern}",
            MaintenanceUrgency.Urgent => $"Schedule maintenance within 48 hours. Primary concern: {primaryConcern}",
            MaintenanceUrgency.Required => $"Plan maintenance within 1-2 weeks. Address: {primaryConcern}",
            MaintenanceUrgency.Scheduled => $"Schedule routine maintenance. Monitor: {primaryConcern}",
            _ => "Continue monitoring. No immediate action required."
        };
    }

    private static double EstimateDowntimeHours(MaintenanceUrgency urgency)
    {
        return urgency switch
        {
            MaintenanceUrgency.Critical => 8,
            MaintenanceUrgency.Urgent => 6,
            MaintenanceUrgency.Required => 4,
            MaintenanceUrgency.Scheduled => 2,
            _ => 1
        };
    }

    private static HealthScoreDto CreateDefaultHealthScore(Guid equipmentId, string name)
    {
        return new HealthScoreDto
        {
            EquipmentId = equipmentId,
            EquipmentName = name,
            EquipmentCode = "UNKNOWN",
            OverallScore = 0,
            ComponentScores = new List<ComponentHealthDto>(),
            DaysUntilMaintenanceRecommended = 0,
            TopConcerns = new List<string> { "Equipment not found" },
            Trend = HealthTrend.Stable
        };
    }
}
