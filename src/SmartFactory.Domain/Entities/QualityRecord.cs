using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a quality inspection record.
/// </summary>
public class QualityRecord : BaseEntity
{
    public Guid EquipmentId { get; private set; }
    public Guid? WorkOrderId { get; private set; }
    public InspectionType InspectionType { get; private set; }
    public InspectionResult Result { get; private set; }
    public DefectType? DefectType { get; private set; }
    public string? DefectDescription { get; private set; }
    public string? InspectorId { get; private set; }
    public string? InspectorName { get; private set; }
    public DateTime InspectedAt { get; private set; }
    public string? ImagePath { get; private set; }
    public string? Notes { get; private set; }
    public int? SampleSize { get; private set; }
    public int? DefectCount { get; private set; }

    public Equipment Equipment { get; private set; } = null!;
    public WorkOrder? WorkOrder { get; private set; }

    // Required for EF Core
    private QualityRecord() { }

    public QualityRecord(
        Guid equipmentId,
        InspectionType inspectionType,
        InspectionResult result,
        DateTime inspectedAt)
    {
        EquipmentId = equipmentId;
        InspectionType = inspectionType;
        Result = result;
        InspectedAt = inspectedAt;
    }

    public void LinkToWorkOrder(Guid workOrderId)
    {
        WorkOrderId = workOrderId;
    }

    public void SetInspector(string inspectorId, string? inspectorName)
    {
        InspectorId = inspectorId;
        InspectorName = inspectorName;
    }

    public void RecordDefect(DefectType defectType, string? description, int count = 1)
    {
        DefectType = defectType;
        DefectDescription = description;
        DefectCount = count;
    }

    public void SetSampleInfo(int sampleSize, int defectCount)
    {
        SampleSize = sampleSize;
        DefectCount = defectCount;
    }

    public void AttachImage(string imagePath)
    {
        ImagePath = imagePath;
    }

    public void AddNotes(string notes)
    {
        Notes = notes;
    }

    public double? DefectRate => SampleSize > 0 && DefectCount.HasValue
        ? (double)DefectCount.Value / SampleSize.Value * 100
        : null;
}
