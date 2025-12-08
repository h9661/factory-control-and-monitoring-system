using SmartFactory.Domain.Enums;

namespace SmartFactory.Application.DTOs.Quality;

/// <summary>
/// Data transfer object for quality record list display.
/// </summary>
public record QualityRecordDto
{
    public Guid Id { get; init; }
    public Guid EquipmentId { get; init; }
    public string EquipmentName { get; init; } = string.Empty;
    public string EquipmentCode { get; init; } = string.Empty;
    public Guid? WorkOrderId { get; init; }
    public string? WorkOrderNumber { get; init; }
    public InspectionType InspectionType { get; init; }
    public InspectionResult Result { get; init; }
    public DefectType? DefectType { get; init; }
    public string? InspectorName { get; init; }
    public DateTime InspectedAt { get; init; }
    public int? SampleSize { get; init; }
    public int? DefectCount { get; init; }
    public double? DefectRate { get; init; }
}

/// <summary>
/// Detailed quality record information.
/// </summary>
public record QualityRecordDetailDto : QualityRecordDto
{
    public string? DefectDescription { get; init; }
    public string? InspectorId { get; init; }
    public string? ImagePath { get; init; }
    public string? Notes { get; init; }
    public string ProductionLineName { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

/// <summary>
/// DTO for recording new inspection.
/// </summary>
public record QualityRecordCreateDto
{
    public Guid EquipmentId { get; init; }
    public Guid? WorkOrderId { get; init; }
    public InspectionType InspectionType { get; init; }
    public InspectionResult Result { get; init; }
    public DefectType? DefectType { get; init; }
    public string? DefectDescription { get; init; }
    public string? InspectorId { get; init; }
    public string? InspectorName { get; init; }
    public int? SampleSize { get; init; }
    public int? DefectCount { get; init; }
    public string? Notes { get; init; }
}

/// <summary>
/// Filter criteria for quality record queries.
/// </summary>
public record QualityFilterDto
{
    public Guid? FactoryId { get; init; }
    public Guid? EquipmentId { get; init; }
    public Guid? WorkOrderId { get; init; }
    public InspectionType? InspectionType { get; init; }
    public InspectionResult? Result { get; init; }
    public DefectType? DefectType { get; init; }
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
    public string? InspectorId { get; init; }
    public string? SearchText { get; init; }
}

/// <summary>
/// Defect summary for reports.
/// </summary>
public record DefectSummaryDto
{
    public int TotalInspections { get; init; }
    public int PassCount { get; init; }
    public int FailCount { get; init; }
    public int TotalDefects { get; init; }
    public double PassRate { get; init; }
    public double OverallDefectRate { get; init; }
    public IEnumerable<DefectTypeCountDto> DefectsByType { get; init; } = Enumerable.Empty<DefectTypeCountDto>();
}

/// <summary>
/// Defect count by type.
/// </summary>
public record DefectTypeCountDto
{
    public DefectType DefectType { get; init; }
    public string DefectTypeName { get; init; } = string.Empty;
    public int Count { get; init; }
    public double Percentage { get; init; }
}

/// <summary>
/// Quality trend data point.
/// </summary>
public record QualityTrendDto
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
