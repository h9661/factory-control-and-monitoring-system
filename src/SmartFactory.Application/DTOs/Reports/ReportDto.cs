namespace SmartFactory.Application.DTOs.Reports;

/// <summary>
/// OEE (Overall Equipment Effectiveness) report.
/// </summary>
public record OeeReportDto
{
    public Guid? FactoryId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public double Availability { get; init; }
    public double Performance { get; init; }
    public double Quality { get; init; }
    public double Oee { get; init; }
    public double OverallOee { get; init; }
    public int TotalPlannedTime { get; init; }
    public int TotalActualTime { get; init; }
    public double TotalProductionTime { get; init; }
    public int TotalDowntime { get; init; }
    public int TotalUnitsProduced { get; init; }
    public int TotalDefects { get; init; }
    public IEnumerable<OeeTrendDto> Trends { get; init; } = Enumerable.Empty<OeeTrendDto>();
}

/// <summary>
/// OEE trend data point.
/// </summary>
public record OeeTrendDto
{
    public DateTime Date { get; init; }
    public double Availability { get; init; }
    public double Performance { get; init; }
    public double Quality { get; init; }
    public double Oee { get; init; }
}

/// <summary>
/// Production report.
/// </summary>
public record ProductionReportDto
{
    public Guid? FactoryId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalWorkOrders { get; init; }
    public int CompletedWorkOrders { get; init; }
    public int InProgressWorkOrders { get; init; }
    public int CancelledWorkOrders { get; init; }
    public int TotalTargetQuantity { get; init; }
    public int TotalCompletedQuantity { get; init; }
    public int TotalDefectQuantity { get; init; }
    public int TotalTargetUnits { get; init; }
    public int TotalCompletedUnits { get; init; }
    public int TotalDefectUnits { get; init; }
    public double YieldRate { get; init; }
    public double OverallYieldRate { get; init; }
    public double OnScheduleRate { get; init; }
    public Dictionary<string, int> WorkOrdersByStatus { get; init; } = new();
    public Dictionary<string, int> WorkOrdersByPriority { get; init; } = new();
    public IEnumerable<ProductionTrendDto> DailyTrends { get; init; } = Enumerable.Empty<ProductionTrendDto>();
    public IEnumerable<ProductionTrendDto> Trends { get; init; } = Enumerable.Empty<ProductionTrendDto>();
    public IEnumerable<ProductProductionDto> TopProducts { get; init; } = Enumerable.Empty<ProductProductionDto>();
    public IEnumerable<ProductionByLineDto> ByProductionLine { get; init; } = Enumerable.Empty<ProductionByLineDto>();
}

/// <summary>
/// Production trend data point.
/// </summary>
public record ProductionTrendDto
{
    public DateTime Date { get; init; }
    public int TargetQuantity { get; init; }
    public int CompletedQuantity { get; init; }
    public int DefectQuantity { get; init; }
    public int WorkOrdersCompleted { get; init; }
    public int TargetUnits { get; init; }
    public int CompletedUnits { get; init; }
    public int DefectUnits { get; init; }
    public double YieldRate { get; init; }
}

/// <summary>
/// Product production data.
/// </summary>
public record ProductProductionDto
{
    public string ProductCode { get; init; } = string.Empty;
    public string ProductName { get; init; } = string.Empty;
    public int WorkOrderCount { get; init; }
    public int TotalQuantity { get; init; }
    public int CompletedQuantity { get; init; }
    public int DefectQuantity { get; init; }
}

/// <summary>
/// Production summary by production line.
/// </summary>
public record ProductionByLineDto
{
    public Guid ProductionLineId { get; init; }
    public string ProductionLineName { get; init; } = string.Empty;
    public int WorkOrderCount { get; init; }
    public int CompletedUnits { get; init; }
    public int DefectUnits { get; init; }
    public double YieldRate { get; init; }
}

/// <summary>
/// Quality report.
/// </summary>
public record QualityReportDto
{
    public Guid? FactoryId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalInspections { get; init; }
    public int PassCount { get; init; }
    public int FailCount { get; init; }
    public double PassRate { get; init; }
    public int TotalDefects { get; init; }
    public double DefectRate { get; init; }
    public Dictionary<string, int> InspectionsByType { get; init; } = new();
    public Dictionary<string, int> DefectsByType { get; init; } = new();
    public IEnumerable<QualityTrendReportDto> DailyTrends { get; init; } = Enumerable.Empty<QualityTrendReportDto>();
    public IEnumerable<QualityTrendReportDto> Trends { get; init; } = Enumerable.Empty<QualityTrendReportDto>();
    public IEnumerable<DefectTypeDto> TopDefects { get; init; } = Enumerable.Empty<DefectTypeDto>();
    public IEnumerable<DefectParetoDto> DefectPareto { get; init; } = Enumerable.Empty<DefectParetoDto>();
}

/// <summary>
/// Defect type data.
/// </summary>
public record DefectTypeDto
{
    public string DefectType { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percentage { get; init; }
}

/// <summary>
/// Quality trend for reports.
/// </summary>
public record QualityTrendReportDto
{
    public DateTime Date { get; init; }
    public int TotalInspections { get; init; }
    public int InspectionCount { get; init; }
    public int PassCount { get; init; }
    public int FailCount { get; init; }
    public int DefectCount { get; init; }
    public double PassRate { get; init; }
    public double DefectRate { get; init; }
}

/// <summary>
/// Defect Pareto analysis data.
/// </summary>
public record DefectParetoDto
{
    public string DefectType { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percentage { get; init; }
    public double CumulativePercentage { get; init; }
}

/// <summary>
/// Maintenance report.
/// </summary>
public record MaintenanceReportDto
{
    public Guid? FactoryId { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalScheduled { get; init; }
    public int TotalCompleted { get; init; }
    public int TotalCancelled { get; init; }
    public int TotalOverdue { get; init; }
    public double CompletionRate { get; init; }
    public decimal TotalEstimatedCost { get; init; }
    public decimal TotalActualCost { get; init; }
    public decimal TotalCost { get; init; }
    public int TotalDowntimeMinutes { get; init; }
    public double AverageDowntimeMinutes { get; init; }
    public Dictionary<string, int> MaintenanceByType { get; init; } = new();
    public Dictionary<string, int> MaintenanceByStatus { get; init; } = new();
    public IEnumerable<MaintenanceTrendDto> DailyTrends { get; init; } = Enumerable.Empty<MaintenanceTrendDto>();
    public IEnumerable<MaintenanceTrendDto> Trends { get; init; } = Enumerable.Empty<MaintenanceTrendDto>();
    public IEnumerable<MaintenanceByTypeDto> ByType { get; init; } = Enumerable.Empty<MaintenanceByTypeDto>();
}

/// <summary>
/// Maintenance trend data point.
/// </summary>
public record MaintenanceTrendDto
{
    public DateTime Date { get; init; }
    public int Scheduled { get; init; }
    public int Completed { get; init; }
    public int ScheduledCount { get; init; }
    public int CompletedCount { get; init; }
    public decimal Cost { get; init; }
    public int DowntimeMinutes { get; init; }
}

/// <summary>
/// Maintenance summary by type.
/// </summary>
public record MaintenanceByTypeDto
{
    public string MaintenanceType { get; init; } = string.Empty;
    public int Count { get; init; }
    public decimal TotalCost { get; init; }
    public int TotalDowntimeMinutes { get; init; }
}

/// <summary>
/// Equipment efficiency data.
/// </summary>
public record EquipmentEfficiencyDto
{
    public Guid EquipmentId { get; init; }
    public string EquipmentCode { get; init; } = string.Empty;
    public string EquipmentName { get; init; } = string.Empty;
    public string EquipmentType { get; init; } = string.Empty;
    public string ProductionLineName { get; init; } = string.Empty;
    public double Availability { get; init; }
    public double Performance { get; init; }
    public double Quality { get; init; }
    public double Oee { get; init; }
    public int TotalOperatingMinutes { get; init; }
    public int TotalRuntime { get; init; }
    public int TotalDowntime { get; init; }
    public int AlarmCount { get; init; }
    public int MaintenanceCount { get; init; }
}

/// <summary>
/// Report date range.
/// </summary>
public record ReportDateRangeDto
{
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}
