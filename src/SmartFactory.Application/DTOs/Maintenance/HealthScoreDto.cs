namespace SmartFactory.Application.DTOs.Maintenance;

/// <summary>
/// Data Transfer Object for equipment health scoring and predictive maintenance.
/// </summary>
public record HealthScoreDto
{
    /// <summary>
    /// Equipment ID.
    /// </summary>
    public Guid EquipmentId { get; init; }

    /// <summary>
    /// Equipment name for display.
    /// </summary>
    public string EquipmentName { get; init; } = string.Empty;

    /// <summary>
    /// Equipment code.
    /// </summary>
    public string EquipmentCode { get; init; } = string.Empty;

    /// <summary>
    /// Overall health score (0-100).
    /// Higher is better.
    /// </summary>
    public double OverallScore { get; init; }

    /// <summary>
    /// Component-level health scores.
    /// </summary>
    public List<ComponentHealthDto> ComponentScores { get; init; } = new();

    /// <summary>
    /// Current risk level based on overall score.
    /// </summary>
    public RiskLevel RiskLevel => OverallScore switch
    {
        >= 80 => RiskLevel.Low,
        >= 60 => RiskLevel.Medium,
        >= 40 => RiskLevel.High,
        _ => RiskLevel.Critical
    };

    /// <summary>
    /// Estimated days until maintenance is recommended.
    /// </summary>
    public int DaysUntilMaintenanceRecommended { get; init; }

    /// <summary>
    /// Top concerns requiring attention.
    /// </summary>
    public List<string> TopConcerns { get; init; } = new();

    /// <summary>
    /// Trending direction of health score.
    /// </summary>
    public HealthTrend Trend { get; init; }

    /// <summary>
    /// Last maintenance date.
    /// </summary>
    public DateTime? LastMaintenanceDate { get; init; }

    /// <summary>
    /// Total operating hours since last maintenance.
    /// </summary>
    public double OperatingHoursSinceLastMaintenance { get; init; }

    /// <summary>
    /// Calculated at timestamp.
    /// </summary>
    public DateTime CalculatedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Component-level health details.
/// </summary>
public record ComponentHealthDto
{
    public string ComponentName { get; init; } = string.Empty;
    public double Score { get; init; }
    public string Status { get; init; } = string.Empty;
    public double TrendValue { get; init; }
    public string? Issue { get; init; }
}

/// <summary>
/// Risk levels for equipment health.
/// </summary>
public enum RiskLevel
{
    Low,
    Medium,
    High,
    Critical
}

/// <summary>
/// Health score trending direction.
/// </summary>
public enum HealthTrend
{
    Improving,
    Stable,
    Declining,
    RapidlyDeclining
}

/// <summary>
/// Maintenance prediction result.
/// </summary>
public record MaintenancePredictionDto
{
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public DateTime PredictedMaintenanceDate { get; init; }
    public int DaysUntilMaintenance { get; init; }
    public double ConfidenceScore { get; init; }
    public MaintenanceUrgency Urgency { get; init; }
    public string RecommendedAction { get; init; } = string.Empty;
    public List<string> WarningIndicators { get; init; } = new();
    public double EstimatedDowntimeHours { get; init; }
}

/// <summary>
/// Maintenance urgency levels.
/// </summary>
public enum MaintenanceUrgency
{
    Routine,        // Can be scheduled flexibly
    Scheduled,      // Should be scheduled soon
    Required,       // Should be done within 1-2 weeks
    Urgent,         // Should be done within days
    Critical        // Immediate attention required
}

/// <summary>
/// Anomaly detection result.
/// </summary>
public record AnomalyResultDto
{
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public string SensorName { get; init; } = string.Empty;
    public double CurrentValue { get; init; }
    public double ExpectedValue { get; init; }
    public double DeviationPercent { get; init; }
    public AnomalySeverity Severity { get; init; }
    public DateTime DetectedAt { get; init; }
    public string Description { get; init; } = string.Empty;
    public bool IsActive { get; init; }
}

/// <summary>
/// Anomaly severity levels.
/// </summary>
public enum AnomalySeverity
{
    Minor,
    Moderate,
    Significant,
    Severe
}

/// <summary>
/// Equipment health trend data point.
/// </summary>
public record HealthTrendPointDto
{
    public DateTime Timestamp { get; init; }
    public double HealthScore { get; init; }
    public double Temperature { get; init; }
    public double Vibration { get; init; }
    public double Pressure { get; init; }
}
