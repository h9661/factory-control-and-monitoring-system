namespace SmartFactory.Application.DTOs.Analytics;

/// <summary>
/// Data Transfer Object for OEE (Overall Equipment Effectiveness) calculation results.
/// OEE = Availability × Performance × Quality
/// </summary>
public record OeeResultDto
{
    /// <summary>
    /// Overall Equipment Effectiveness score (0-100%).
    /// Calculated as: Availability × Performance × Quality
    /// </summary>
    public double OverallOee { get; init; }

    /// <summary>
    /// Availability rate (0-100%).
    /// Calculated as: Run Time / Planned Production Time
    /// </summary>
    public double Availability { get; init; }

    /// <summary>
    /// Performance rate (0-100%).
    /// Calculated as: (Ideal Cycle Time × Total Count) / Run Time
    /// </summary>
    public double Performance { get; init; }

    /// <summary>
    /// Quality rate (0-100%).
    /// Calculated as: Good Count / Total Count
    /// </summary>
    public double Quality { get; init; }

    /// <summary>
    /// Total planned production time in minutes.
    /// </summary>
    public double PlannedProductionTimeMinutes { get; init; }

    /// <summary>
    /// Actual run time in minutes (excluding downtime).
    /// </summary>
    public double ActualRunTimeMinutes { get; init; }

    /// <summary>
    /// Idle time in minutes (scheduled breaks, no orders).
    /// </summary>
    public double IdleTimeMinutes { get; init; }

    /// <summary>
    /// Unplanned downtime in minutes (breakdowns, changeovers).
    /// </summary>
    public double DownTimeMinutes { get; init; }

    /// <summary>
    /// Total units produced (good + defects).
    /// </summary>
    public int TotalProduced { get; init; }

    /// <summary>
    /// Number of good units produced.
    /// </summary>
    public int GoodUnits { get; init; }

    /// <summary>
    /// Number of defective units.
    /// </summary>
    public int DefectUnits { get; init; }

    /// <summary>
    /// Ideal cycle time in seconds per unit.
    /// </summary>
    public double IdealCycleTimeSeconds { get; init; }

    /// <summary>
    /// Equipment ID for this OEE calculation.
    /// </summary>
    public Guid? EquipmentId { get; init; }

    /// <summary>
    /// Equipment name for display purposes.
    /// </summary>
    public string? EquipmentName { get; init; }

    /// <summary>
    /// Start of the calculation period.
    /// </summary>
    public DateTime PeriodStart { get; init; }

    /// <summary>
    /// End of the calculation period.
    /// </summary>
    public DateTime PeriodEnd { get; init; }

    /// <summary>
    /// Classification of OEE score.
    /// </summary>
    public OeeClassification Classification => OverallOee switch
    {
        >= 85 => OeeClassification.WorldClass,
        >= 75 => OeeClassification.Good,
        >= 60 => OeeClassification.Average,
        >= 40 => OeeClassification.NeedsImprovement,
        _ => OeeClassification.Poor
    };
}

/// <summary>
/// Classification levels for OEE scores.
/// </summary>
public enum OeeClassification
{
    WorldClass,      // >= 85%
    Good,            // 75-84%
    Average,         // 60-74%
    NeedsImprovement, // 40-59%
    Poor             // < 40%
}

/// <summary>
/// Data point for OEE trend charts.
/// </summary>
public record OeeDataPointDto
{
    public DateTime Timestamp { get; init; }
    public double OverallOee { get; init; }
    public double Availability { get; init; }
    public double Performance { get; init; }
    public double Quality { get; init; }
}

/// <summary>
/// Loss breakdown for OEE analysis.
/// </summary>
public record OeeLossBreakdownDto
{
    /// <summary>
    /// Availability losses (downtime, changeovers).
    /// </summary>
    public double AvailabilityLossPercent { get; init; }

    /// <summary>
    /// Performance losses (slow cycles, minor stops).
    /// </summary>
    public double PerformanceLossPercent { get; init; }

    /// <summary>
    /// Quality losses (defects, rework).
    /// </summary>
    public double QualityLossPercent { get; init; }

    /// <summary>
    /// Total effective production percentage.
    /// </summary>
    public double EffectiveProductionPercent { get; init; }

    /// <summary>
    /// Categories of losses for detailed breakdown.
    /// </summary>
    public List<LossCategoryDto> LossCategories { get; init; } = new();
}

/// <summary>
/// Detailed loss category for analysis.
/// </summary>
public record LossCategoryDto
{
    public string Name { get; init; } = string.Empty;
    public LossType Type { get; init; }
    public double LossMinutes { get; init; }
    public double LossPercent { get; init; }
}

/// <summary>
/// Types of production losses.
/// </summary>
public enum LossType
{
    Availability,
    Performance,
    Quality
}

/// <summary>
/// Comparison data for shift/day OEE analysis.
/// </summary>
public record OeeComparisonDto
{
    public string Label { get; init; } = string.Empty;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public double OverallOee { get; init; }
    public double Availability { get; init; }
    public double Performance { get; init; }
    public double Quality { get; init; }
    public int TotalProduced { get; init; }
    public int GoodUnits { get; init; }
    public double ChangeFromPrevious { get; init; }
}
