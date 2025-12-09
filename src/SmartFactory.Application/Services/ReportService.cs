using Microsoft.Extensions.Logging;
using SmartFactory.Application.DTOs.Reports;
using SmartFactory.Application.Interfaces;
using SmartFactory.Domain.Enums;
using SmartFactory.Domain.Interfaces;
using SmartFactory.Domain.ValueObjects;

namespace SmartFactory.Application.Services;

/// <summary>
/// Service implementation for report generation.
/// </summary>
public class ReportService : IReportService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IEquipmentRepository _equipmentRepository;
    private readonly IMaintenanceRepository _maintenanceRepository;
    private readonly IQualityRecordRepository _qualityRecordRepository;
    private readonly ILogger<ReportService> _logger;

    public ReportService(
        IWorkOrderRepository workOrderRepository,
        IEquipmentRepository equipmentRepository,
        IMaintenanceRepository maintenanceRepository,
        IQualityRecordRepository qualityRecordRepository,
        ILogger<ReportService> logger)
    {
        _workOrderRepository = workOrderRepository;
        _equipmentRepository = equipmentRepository;
        _maintenanceRepository = maintenanceRepository;
        _qualityRecordRepository = qualityRecordRepository;
        _logger = logger;
    }

    public async Task<OeeReportDto> GenerateOeeReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating OEE report for factory {FactoryId} from {StartDate} to {EndDate}",
            factoryId, startDate, endDate);

        // Get work orders for the period
        var workOrders = await _workOrderRepository.GetByDateRangeAsync(
            new DateTimeRange(startDate, endDate), cancellationToken);
        if (factoryId.HasValue)
            workOrders = workOrders.Where(w => w.FactoryId == factoryId.Value);

        var workOrderList = workOrders.ToList();

        // Get maintenance records for downtime calculation
        var maintenanceRecords = factoryId.HasValue
            ? await _maintenanceRepository.GetByFactoryAsync(factoryId.Value, cancellationToken)
            : await _maintenanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);

        maintenanceRecords = maintenanceRecords.Where(m =>
            m.ScheduledDate >= startDate && m.ScheduledDate <= endDate);

        // Get quality records for quality calculation
        var qualityStats = await _qualityRecordRepository.GetDefectStatisticsAsync(factoryId, startDate, endDate, cancellationToken);

        // Calculate metrics
        var totalPlannedTime = (endDate - startDate).TotalMinutes;
        var totalDowntime = maintenanceRecords.Sum(m => m.DowntimeMinutes ?? 0);
        var operatingTime = totalPlannedTime - totalDowntime;

        // Availability = Operating Time / Planned Time
        var availability = totalPlannedTime > 0 ? operatingTime / totalPlannedTime * 100 : 0;

        // Performance = (Total Units / Operating Time) / Ideal Cycle Time
        var totalTargetUnits = workOrderList.Sum(w => w.TargetQuantity);
        var totalCompletedUnits = workOrderList.Sum(w => w.CompletedQuantity);
        var performance = totalTargetUnits > 0 ? (double)totalCompletedUnits / totalTargetUnits * 100 : 0;

        // Quality = Good Units / Total Units
        var quality = qualityStats.TotalInspections > 0 ? qualityStats.PassRate : 100;

        // OEE = Availability × Performance × Quality / 10000
        var oee = (availability * performance * quality) / 10000;

        var trends = await GenerateOeeTrendsAsync(factoryId, startDate, endDate, cancellationToken);

        return new OeeReportDto
        {
            FactoryId = factoryId,
            StartDate = startDate,
            EndDate = endDate,
            Availability = Math.Round(availability, 2),
            Performance = Math.Round(performance, 2),
            Quality = Math.Round(quality, 2),
            OverallOee = Math.Round(oee, 2),
            TotalProductionTime = operatingTime,
            TotalDowntime = totalDowntime,
            TotalUnitsProduced = totalCompletedUnits,
            TotalDefects = qualityStats.TotalDefects,
            Trends = trends.ToList()
        };
    }

    public async Task<ProductionReportDto> GenerateProductionReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating production report for factory {FactoryId} from {StartDate} to {EndDate}",
            factoryId, startDate, endDate);

        var workOrders = await _workOrderRepository.GetByDateRangeAsync(
            new DateTimeRange(startDate, endDate), cancellationToken);
        if (factoryId.HasValue)
            workOrders = workOrders.Where(w => w.FactoryId == factoryId.Value);

        var workOrderList = workOrders.ToList();

        var byStatus = workOrderList
            .GroupBy(w => w.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var byPriority = workOrderList
            .GroupBy(w => w.Priority)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var dailyTrends = workOrderList
            .GroupBy(w => w.CreatedAt.Date)
            .Select(g => new ProductionTrendDto
            {
                Date = g.Key,
                TargetQuantity = g.Sum(w => w.TargetQuantity),
                CompletedQuantity = g.Sum(w => w.CompletedQuantity),
                DefectQuantity = g.Sum(w => w.DefectQuantity),
                WorkOrdersCompleted = g.Count(w => w.Status == WorkOrderStatus.Completed)
            })
            .OrderBy(t => t.Date)
            .ToList();

        var topProducts = workOrderList
            .GroupBy(w => w.ProductCode)
            .Select(g => new ProductProductionDto
            {
                ProductCode = g.Key,
                ProductName = g.First().ProductName,
                WorkOrderCount = g.Count(),
                TotalQuantity = g.Sum(w => w.TargetQuantity),
                CompletedQuantity = g.Sum(w => w.CompletedQuantity),
                DefectQuantity = g.Sum(w => w.DefectQuantity)
            })
            .OrderByDescending(p => p.CompletedQuantity)
            .Take(10)
            .ToList();

        return new ProductionReportDto
        {
            FactoryId = factoryId,
            StartDate = startDate,
            EndDate = endDate,
            TotalWorkOrders = workOrderList.Count,
            CompletedWorkOrders = workOrderList.Count(w => w.Status == WorkOrderStatus.Completed),
            InProgressWorkOrders = workOrderList.Count(w => w.Status == WorkOrderStatus.InProgress),
            TotalTargetQuantity = workOrderList.Sum(w => w.TargetQuantity),
            TotalCompletedQuantity = workOrderList.Sum(w => w.CompletedQuantity),
            TotalDefectQuantity = workOrderList.Sum(w => w.DefectQuantity),
            YieldRate = workOrderList.Sum(w => w.TargetQuantity) > 0
                ? Math.Round((double)workOrderList.Sum(w => w.CompletedQuantity) / workOrderList.Sum(w => w.TargetQuantity) * 100, 2)
                : 0,
            WorkOrdersByStatus = byStatus,
            WorkOrdersByPriority = byPriority,
            DailyTrends = dailyTrends,
            TopProducts = topProducts
        };
    }

    public async Task<QualityReportDto> GenerateQualityReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating quality report for factory {FactoryId} from {StartDate} to {EndDate}",
            factoryId, startDate, endDate);

        var statistics = await _qualityRecordRepository.GetDefectStatisticsAsync(factoryId, startDate, endDate, cancellationToken);

        IEnumerable<Domain.Entities.QualityRecord> records;
        if (factoryId.HasValue)
        {
            records = await _qualityRecordRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            records = records.Where(r => r.InspectedAt >= startDate && r.InspectedAt <= endDate);
        }
        else
        {
            records = await _qualityRecordRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        var recordList = records.ToList();

        var byInspectionType = recordList
            .GroupBy(r => r.InspectionType)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var dailyTrends = recordList
            .GroupBy(r => r.InspectedAt.Date)
            .Select(g => new QualityTrendReportDto
            {
                Date = g.Key,
                TotalInspections = g.Count(),
                PassCount = g.Count(r => r.Result == InspectionResult.Pass),
                FailCount = g.Count(r => r.Result == InspectionResult.Fail),
                DefectCount = g.Sum(r => r.DefectCount ?? 0)
            })
            .OrderBy(t => t.Date)
            .ToList();

        var topDefects = statistics.DefectsByType
            .OrderByDescending(d => d.Value)
            .Take(10)
            .Select(d => new DefectTypeDto
            {
                DefectType = d.Key.ToString(),
                Count = d.Value,
                Percentage = statistics.TotalDefects > 0
                    ? Math.Round((double)d.Value / statistics.TotalDefects * 100, 2)
                    : 0
            })
            .ToList();

        return new QualityReportDto
        {
            FactoryId = factoryId,
            StartDate = startDate,
            EndDate = endDate,
            TotalInspections = statistics.TotalInspections,
            PassCount = statistics.PassCount,
            FailCount = statistics.FailCount,
            TotalDefects = statistics.TotalDefects,
            PassRate = Math.Round(statistics.PassRate, 2),
            InspectionsByType = byInspectionType,
            DefectsByType = statistics.DefectsByType.ToDictionary(k => k.Key.ToString(), v => v.Value),
            DailyTrends = dailyTrends,
            TopDefects = topDefects
        };
    }

    public async Task<MaintenanceReportDto> GenerateMaintenanceReportAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating maintenance report for factory {FactoryId} from {StartDate} to {EndDate}",
            factoryId, startDate, endDate);

        IEnumerable<Domain.Entities.MaintenanceRecord> records;
        if (factoryId.HasValue)
        {
            records = await _maintenanceRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
            records = records.Where(m => m.ScheduledDate >= startDate && m.ScheduledDate <= endDate);
        }
        else
        {
            records = await _maintenanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);
        }

        var recordList = records.ToList();
        var completedRecords = recordList.Where(m => m.Status == MaintenanceStatus.Completed).ToList();

        var byType = recordList
            .GroupBy(m => m.Type)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var byStatus = recordList
            .GroupBy(m => m.Status)
            .ToDictionary(g => g.Key.ToString(), g => g.Count());

        var dailyTrends = recordList
            .GroupBy(m => m.ScheduledDate.Date)
            .Select(g => new MaintenanceTrendDto
            {
                Date = g.Key,
                Scheduled = g.Count(),
                Completed = g.Count(m => m.Status == MaintenanceStatus.Completed),
                DowntimeMinutes = g.Sum(m => m.DowntimeMinutes ?? 0),
                Cost = g.Sum(m => m.ActualCost ?? 0)
            })
            .OrderBy(t => t.Date)
            .ToList();

        return new MaintenanceReportDto
        {
            FactoryId = factoryId,
            StartDate = startDate,
            EndDate = endDate,
            TotalScheduled = recordList.Count,
            TotalCompleted = completedRecords.Count,
            TotalOverdue = recordList.Count(m => m.Status == MaintenanceStatus.Overdue),
            TotalCancelled = recordList.Count(m => m.Status == MaintenanceStatus.Cancelled),
            TotalDowntimeMinutes = completedRecords.Sum(m => m.DowntimeMinutes ?? 0),
            TotalEstimatedCost = recordList.Sum(m => m.EstimatedCost ?? 0),
            TotalActualCost = completedRecords.Sum(m => m.ActualCost ?? 0),
            CompletionRate = recordList.Count > 0
                ? Math.Round((double)completedRecords.Count / recordList.Count * 100, 2)
                : 0,
            MaintenanceByType = byType,
            MaintenanceByStatus = byStatus,
            DailyTrends = dailyTrends
        };
    }

    public async Task<IEnumerable<EquipmentEfficiencyDto>> GetEquipmentEfficiencyAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting equipment efficiency for factory {FactoryId} from {StartDate} to {EndDate}",
            factoryId, startDate, endDate);

        IEnumerable<Domain.Entities.Equipment> equipment;
        if (factoryId.HasValue)
        {
            equipment = await _equipmentRepository.GetByFactoryAsync(factoryId.Value, cancellationToken);
        }
        else
        {
            equipment = await _equipmentRepository.GetAllAsync(cancellationToken);
        }

        var maintenanceRecords = factoryId.HasValue
            ? await _maintenanceRepository.GetByFactoryAsync(factoryId.Value, cancellationToken)
            : await _maintenanceRepository.GetByDateRangeAsync(startDate, endDate, cancellationToken);

        // Create lookup dictionary for O(1) access instead of O(n*m) filtering
        var maintenanceByEquipment = maintenanceRecords
            .Where(m => m.ScheduledDate >= startDate && m.ScheduledDate <= endDate)
            .GroupBy(m => m.EquipmentId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var efficiencyList = new List<EquipmentEfficiencyDto>();
        var totalMinutes = (endDate - startDate).TotalMinutes;

        foreach (var eq in equipment)
        {
            // O(1) lookup instead of O(m) filtering
            var eqMaintenance = maintenanceByEquipment.GetValueOrDefault(eq.Id, new List<Domain.Entities.MaintenanceRecord>());

            var downtime = eqMaintenance.Sum(m => m.DowntimeMinutes ?? 0);
            var operatingTime = totalMinutes - downtime;
            var availability = totalMinutes > 0 ? operatingTime / totalMinutes * 100 : 0;

            efficiencyList.Add(new EquipmentEfficiencyDto
            {
                EquipmentId = eq.Id,
                EquipmentCode = eq.Code,
                EquipmentName = eq.Name,
                EquipmentType = eq.Type.ToString(),
                TotalOperatingMinutes = (int)operatingTime,
                TotalDowntime = downtime,
                MaintenanceCount = eqMaintenance.Count,
                Availability = Math.Round(availability, 2)
            });
        }

        return efficiencyList.OrderByDescending(e => e.Availability);
    }

    private async Task<IEnumerable<OeeTrendDto>> GenerateOeeTrendsAsync(Guid? factoryId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken)
    {
        var trends = new List<OeeTrendDto>();
        var currentDate = startDate.Date;

        while (currentDate <= endDate.Date)
        {
            var nextDate = currentDate.AddDays(1);

            var workOrders = await _workOrderRepository.GetByDateRangeAsync(
                new DateTimeRange(currentDate, nextDate), cancellationToken);
            if (factoryId.HasValue)
                workOrders = workOrders.Where(w => w.FactoryId == factoryId.Value);

            var workOrderList = workOrders.ToList();
            var totalTarget = workOrderList.Sum(w => w.TargetQuantity);
            var totalCompleted = workOrderList.Sum(w => w.CompletedQuantity);

            var quality = await _qualityRecordRepository.GetDefectStatisticsAsync(factoryId, currentDate, nextDate, cancellationToken);

            // Simplified OEE calculation for daily trends
            var performance = totalTarget > 0 ? (double)totalCompleted / totalTarget * 100 : 100;
            var qualityRate = quality.TotalInspections > 0 ? quality.PassRate : 100;
            var availability = 100.0; // Simplified assumption

            var oee = (availability * performance * qualityRate) / 10000;

            trends.Add(new OeeTrendDto
            {
                Date = currentDate,
                Availability = Math.Round(availability, 2),
                Performance = Math.Round(performance, 2),
                Quality = Math.Round(qualityRate, 2),
                Oee = Math.Round(oee, 2)
            });

            currentDate = nextDate;
        }

        return trends;
    }
}
