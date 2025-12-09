using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Analytics;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;

namespace SmartFactory.Application.Services.Analytics;

/// <summary>
/// Service for calculating OEE (Overall Equipment Effectiveness) metrics.
/// OEE = Availability × Performance × Quality
/// </summary>
public class OeeCalculationService : IOeeCalculationService
{
    private readonly ILogger<OeeCalculationService> _logger;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IWorkOrderRepository _workOrderRepository;

    // Default ideal cycle time in seconds (can be overridden per equipment)
    private const double DefaultIdealCycleTimeSeconds = 30.0;

    // Standard shift duration in hours
    private const double StandardShiftHours = 8.0;

    public OeeCalculationService(
        ILogger<OeeCalculationService> logger,
        IEquipmentRepository equipmentRepository,
        IWorkOrderRepository workOrderRepository)
    {
        _logger = logger;
        _equipmentRepository = equipmentRepository;
        _workOrderRepository = workOrderRepository;
    }

    public async Task<OeeResultDto> CalculateOeeAsync(
        Guid equipmentId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByIdAsync(equipmentId, cancellationToken);
        if (equipment == null)
        {
            _logger.LogWarning("Equipment {EquipmentId} not found for OEE calculation", equipmentId);
            return CreateEmptyResult(equipmentId, null, startDate, endDate);
        }

        // Get production data for the period
        var productionSummary = await _workOrderRepository.GetProductionSummaryAsync(
            equipment.ProductionLine?.FactoryId,
            startDate,
            cancellationToken);

        // Calculate time components
        var totalPeriodMinutes = (endDate - startDate).TotalMinutes;
        var plannedProductionMinutes = totalPeriodMinutes * 0.9; // 90% planned production time

        // Simulate realistic data based on equipment status
        var (runTimeMinutes, idleMinutes, downTimeMinutes) = CalculateTimeBreakdown(
            equipment.Status,
            plannedProductionMinutes);

        // Get production counts from summary
        var totalProduced = productionSummary.CompletedUnits;
        var defectUnits = productionSummary.DefectUnits;
        var goodUnits = totalProduced - defectUnits;

        // Calculate OEE components
        var availability = plannedProductionMinutes > 0
            ? (runTimeMinutes / plannedProductionMinutes) * 100
            : 0;

        var idealCycleTime = DefaultIdealCycleTimeSeconds;
        var performance = runTimeMinutes > 0
            ? ((idealCycleTime * totalProduced) / (runTimeMinutes * 60)) * 100
            : 0;

        var quality = totalProduced > 0
            ? ((double)goodUnits / totalProduced) * 100
            : 0;

        // Clamp values to valid ranges
        availability = Math.Clamp(availability, 0, 100);
        performance = Math.Clamp(performance, 0, 100);
        quality = Math.Clamp(quality, 0, 100);

        var overallOee = (availability / 100) * (performance / 100) * (quality / 100) * 100;

        return new OeeResultDto
        {
            OverallOee = Math.Round(overallOee, 2),
            Availability = Math.Round(availability, 2),
            Performance = Math.Round(performance, 2),
            Quality = Math.Round(quality, 2),
            PlannedProductionTimeMinutes = plannedProductionMinutes,
            ActualRunTimeMinutes = runTimeMinutes,
            IdleTimeMinutes = idleMinutes,
            DownTimeMinutes = downTimeMinutes,
            TotalProduced = totalProduced,
            GoodUnits = goodUnits,
            DefectUnits = defectUnits,
            IdealCycleTimeSeconds = idealCycleTime,
            EquipmentId = equipmentId,
            EquipmentName = equipment.Name,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }

    public async Task<OeeResultDto> CalculateFactoryOeeAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        // Get all equipment in the factory
        var equipment = await _equipmentRepository.GetByFactoryAsync(factoryId, cancellationToken);
        var activeEquipment = equipment.Where(e => e.IsActive).ToList();

        if (!activeEquipment.Any())
        {
            return CreateEmptyResult(null, null, startDate, endDate);
        }

        // Get production summary
        var productionSummary = await _workOrderRepository.GetProductionSummaryAsync(
            factoryId,
            startDate,
            cancellationToken);

        // Calculate aggregate metrics
        var totalPeriodMinutes = (endDate - startDate).TotalMinutes;
        var totalPlannedMinutes = totalPeriodMinutes * activeEquipment.Count * 0.9;

        double totalRunTime = 0;
        double totalIdleTime = 0;
        double totalDownTime = 0;

        foreach (var eq in activeEquipment)
        {
            var plannedMinutes = totalPeriodMinutes * 0.9;
            var (runTime, idleTime, downTime) = CalculateTimeBreakdown(eq.Status, plannedMinutes);
            totalRunTime += runTime;
            totalIdleTime += idleTime;
            totalDownTime += downTime;
        }

        var totalProduced = productionSummary.CompletedUnits;
        var defectUnits = productionSummary.DefectUnits;
        var goodUnits = totalProduced - defectUnits;

        // Calculate OEE components
        var availability = totalPlannedMinutes > 0
            ? (totalRunTime / totalPlannedMinutes) * 100
            : 0;

        var performance = totalRunTime > 0
            ? ((DefaultIdealCycleTimeSeconds * totalProduced) / (totalRunTime * 60)) * 100
            : 0;

        var quality = totalProduced > 0
            ? ((double)goodUnits / totalProduced) * 100
            : 0;

        availability = Math.Clamp(availability, 0, 100);
        performance = Math.Clamp(performance, 0, 100);
        quality = Math.Clamp(quality, 0, 100);

        var overallOee = (availability / 100) * (performance / 100) * (quality / 100) * 100;

        return new OeeResultDto
        {
            OverallOee = Math.Round(overallOee, 2),
            Availability = Math.Round(availability, 2),
            Performance = Math.Round(performance, 2),
            Quality = Math.Round(quality, 2),
            PlannedProductionTimeMinutes = totalPlannedMinutes,
            ActualRunTimeMinutes = totalRunTime,
            IdleTimeMinutes = totalIdleTime,
            DownTimeMinutes = totalDownTime,
            TotalProduced = totalProduced,
            GoodUnits = goodUnits,
            DefectUnits = defectUnits,
            IdealCycleTimeSeconds = DefaultIdealCycleTimeSeconds,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }

    public async Task<List<OeeDataPointDto>> GetOeeTrendAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        OeeGranularity granularity = OeeGranularity.Hourly,
        CancellationToken cancellationToken = default)
    {
        var dataPoints = new List<OeeDataPointDto>();
        var interval = granularity switch
        {
            OeeGranularity.Hourly => TimeSpan.FromHours(1),
            OeeGranularity.Shift => TimeSpan.FromHours(8),
            OeeGranularity.Daily => TimeSpan.FromDays(1),
            OeeGranularity.Weekly => TimeSpan.FromDays(7),
            OeeGranularity.Monthly => TimeSpan.FromDays(30),
            _ => TimeSpan.FromHours(1)
        };

        var current = startDate;
        var random = new Random((int)DateTime.Now.Ticks);

        // Generate realistic trend data with variations
        var baseOee = 72.0; // Industry average starting point
        var baseAvailability = 88.0;
        var basePerformance = 90.0;
        var baseQuality = 95.0;

        while (current < endDate)
        {
            // Add realistic variations
            var hourOfDay = current.Hour;
            var dayOfWeek = (int)current.DayOfWeek;

            // Performance tends to drop during night shifts and weekends
            var timeModifier = hourOfDay >= 6 && hourOfDay <= 18 ? 1.0 : 0.92;
            var weekendModifier = dayOfWeek == 0 || dayOfWeek == 6 ? 0.85 : 1.0;

            var availability = baseAvailability * timeModifier * weekendModifier
                + (random.NextDouble() - 0.5) * 10;
            var performance = basePerformance * timeModifier
                + (random.NextDouble() - 0.5) * 8;
            var quality = baseQuality
                + (random.NextDouble() - 0.5) * 4;

            availability = Math.Clamp(availability, 60, 100);
            performance = Math.Clamp(performance, 65, 100);
            quality = Math.Clamp(quality, 85, 100);

            var oee = (availability / 100) * (performance / 100) * (quality / 100) * 100;

            dataPoints.Add(new OeeDataPointDto
            {
                Timestamp = current,
                OverallOee = Math.Round(oee, 2),
                Availability = Math.Round(availability, 2),
                Performance = Math.Round(performance, 2),
                Quality = Math.Round(quality, 2)
            });

            current += interval;
        }

        return dataPoints;
    }

    public async Task<OeeLossBreakdownDto> GetLossBreakdownAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var factoryOee = await CalculateFactoryOeeAsync(factoryId, startDate, endDate, cancellationToken);

        // Calculate loss percentages
        var availabilityLoss = 100 - factoryOee.Availability;
        var performanceLossBase = factoryOee.Availability / 100;
        var performanceLoss = performanceLossBase * (100 - factoryOee.Performance);
        var qualityLossBase = performanceLossBase * (factoryOee.Performance / 100);
        var qualityLoss = qualityLossBase * (100 - factoryOee.Quality);

        var effectiveProduction = factoryOee.OverallOee;

        var lossCategories = new List<LossCategoryDto>
        {
            new()
            {
                Name = "Equipment Breakdown",
                Type = LossType.Availability,
                LossMinutes = factoryOee.DownTimeMinutes * 0.6,
                LossPercent = availabilityLoss * 0.5
            },
            new()
            {
                Name = "Setup & Changeover",
                Type = LossType.Availability,
                LossMinutes = factoryOee.DownTimeMinutes * 0.3,
                LossPercent = availabilityLoss * 0.35
            },
            new()
            {
                Name = "Other Downtime",
                Type = LossType.Availability,
                LossMinutes = factoryOee.DownTimeMinutes * 0.1,
                LossPercent = availabilityLoss * 0.15
            },
            new()
            {
                Name = "Minor Stops",
                Type = LossType.Performance,
                LossMinutes = factoryOee.IdleTimeMinutes * 0.4,
                LossPercent = performanceLoss * 0.45
            },
            new()
            {
                Name = "Slow Cycles",
                Type = LossType.Performance,
                LossMinutes = factoryOee.IdleTimeMinutes * 0.4,
                LossPercent = performanceLoss * 0.55
            },
            new()
            {
                Name = "Defects & Rework",
                Type = LossType.Quality,
                LossMinutes = factoryOee.DefectUnits * (DefaultIdealCycleTimeSeconds / 60),
                LossPercent = qualityLoss * 0.7
            },
            new()
            {
                Name = "Startup Rejects",
                Type = LossType.Quality,
                LossMinutes = factoryOee.DefectUnits * 0.3 * (DefaultIdealCycleTimeSeconds / 60),
                LossPercent = qualityLoss * 0.3
            }
        };

        return new OeeLossBreakdownDto
        {
            AvailabilityLossPercent = Math.Round(availabilityLoss, 2),
            PerformanceLossPercent = Math.Round(performanceLoss, 2),
            QualityLossPercent = Math.Round(qualityLoss, 2),
            EffectiveProductionPercent = Math.Round(effectiveProduction, 2),
            LossCategories = lossCategories.Select(c => c with
            {
                LossMinutes = Math.Round(c.LossMinutes, 2),
                LossPercent = Math.Round(c.LossPercent, 2)
            }).ToList()
        };
    }

    public async Task<List<OeeComparisonDto>> GetOeeComparisonAsync(
        Guid factoryId,
        OeeComparisonType comparisonType,
        int periodCount = 7,
        CancellationToken cancellationToken = default)
    {
        var comparisons = new List<OeeComparisonDto>();
        var now = DateTime.UtcNow;

        var (periodDuration, labelFormat) = comparisonType switch
        {
            OeeComparisonType.Shift => (TimeSpan.FromHours(8), "Shift {0}"),
            OeeComparisonType.Day => (TimeSpan.FromDays(1), "{0:ddd MM/dd}"),
            OeeComparisonType.Week => (TimeSpan.FromDays(7), "Week {0}"),
            OeeComparisonType.Month => (TimeSpan.FromDays(30), "{0:MMM yyyy}"),
            _ => (TimeSpan.FromDays(1), "{0:ddd MM/dd}")
        };

        double? previousOee = null;

        for (int i = periodCount - 1; i >= 0; i--)
        {
            var periodEnd = now - (periodDuration * i);
            var periodStart = periodEnd - periodDuration;

            var oeeResult = await CalculateFactoryOeeAsync(
                factoryId,
                periodStart,
                periodEnd,
                cancellationToken);

            var label = comparisonType switch
            {
                OeeComparisonType.Shift => string.Format(labelFormat, periodCount - i),
                OeeComparisonType.Week => string.Format(labelFormat, periodCount - i),
                _ => string.Format(labelFormat, periodStart)
            };

            var changeFromPrevious = previousOee.HasValue
                ? oeeResult.OverallOee - previousOee.Value
                : 0;

            comparisons.Add(new OeeComparisonDto
            {
                Label = label,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                OverallOee = oeeResult.OverallOee,
                Availability = oeeResult.Availability,
                Performance = oeeResult.Performance,
                Quality = oeeResult.Quality,
                TotalProduced = oeeResult.TotalProduced,
                GoodUnits = oeeResult.GoodUnits,
                ChangeFromPrevious = Math.Round(changeFromPrevious, 2)
            });

            previousOee = oeeResult.OverallOee;
        }

        return comparisons;
    }

    public async Task<List<OeeResultDto>> GetEquipmentOeeListAsync(
        Guid factoryId,
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var equipment = await _equipmentRepository.GetByFactoryAsync(factoryId, cancellationToken);
        var activeEquipment = equipment.Where(e => e.IsActive).ToList();

        var results = new List<OeeResultDto>();

        foreach (var eq in activeEquipment)
        {
            var oeeResult = await CalculateOeeAsync(eq.Id, startDate, endDate, cancellationToken);
            results.Add(oeeResult);
        }

        return results.OrderByDescending(r => r.OverallOee).ToList();
    }

    private static (double runTime, double idleTime, double downTime) CalculateTimeBreakdown(
        EquipmentStatus status,
        double plannedMinutes)
    {
        // Realistic time distribution based on equipment status
        var (runPercent, idlePercent, downPercent) = status switch
        {
            EquipmentStatus.Running => (0.85, 0.10, 0.05),
            EquipmentStatus.Idle => (0.20, 0.70, 0.10),
            EquipmentStatus.Warning => (0.60, 0.20, 0.20),
            EquipmentStatus.Error => (0.10, 0.15, 0.75),
            EquipmentStatus.Maintenance => (0.05, 0.10, 0.85),
            EquipmentStatus.Offline => (0.00, 0.00, 1.00),
            _ => (0.50, 0.30, 0.20)
        };

        return (
            plannedMinutes * runPercent,
            plannedMinutes * idlePercent,
            plannedMinutes * downPercent
        );
    }

    private static OeeResultDto CreateEmptyResult(
        Guid? equipmentId,
        string? equipmentName,
        DateTime startDate,
        DateTime endDate)
    {
        return new OeeResultDto
        {
            OverallOee = 0,
            Availability = 0,
            Performance = 0,
            Quality = 0,
            PlannedProductionTimeMinutes = 0,
            ActualRunTimeMinutes = 0,
            IdleTimeMinutes = 0,
            DownTimeMinutes = 0,
            TotalProduced = 0,
            GoodUnits = 0,
            DefectUnits = 0,
            IdealCycleTimeSeconds = DefaultIdealCycleTimeSeconds,
            EquipmentId = equipmentId,
            EquipmentName = equipmentName,
            PeriodStart = startDate,
            PeriodEnd = endDate
        };
    }
}
