using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents an equipment alarm/alert.
/// </summary>
public class Alarm : BaseEntity
{
    public Guid EquipmentId { get; private set; }
    public string AlarmCode { get; private set; } = string.Empty;
    public AlarmSeverity Severity { get; private set; }
    public AlarmStatus Status { get; private set; } = AlarmStatus.Active;
    public string Message { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public DateTime OccurredAt { get; private set; }
    public DateTime? AcknowledgedAt { get; private set; }
    public string? AcknowledgedBy { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    public string? ResolvedBy { get; private set; }
    public string? ResolutionNotes { get; private set; }

    public Equipment Equipment { get; private set; } = null!;

    // Required for EF Core
    private Alarm() { }

    public Alarm(
        Guid equipmentId,
        string alarmCode,
        AlarmSeverity severity,
        string message,
        DateTime occurredAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alarmCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);

        EquipmentId = equipmentId;
        AlarmCode = alarmCode;
        Severity = severity;
        Message = message;
        OccurredAt = occurredAt;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    public void Acknowledge(string userId)
    {
        if (Status != AlarmStatus.Active)
            throw new InvalidOperationException("Can only acknowledge active alarms.");

        Status = AlarmStatus.Acknowledged;
        AcknowledgedAt = DateTime.UtcNow;
        AcknowledgedBy = userId;
    }

    public void Resolve(string userId, string? notes = null)
    {
        Status = AlarmStatus.Resolved;
        ResolvedAt = DateTime.UtcNow;
        ResolvedBy = userId;
        ResolutionNotes = notes;
    }

    public bool IsActive => Status == AlarmStatus.Active;
    public bool IsAcknowledged => Status == AlarmStatus.Acknowledged;
    public bool IsResolved => Status == AlarmStatus.Resolved;

    public TimeSpan? TimeToAcknowledge => AcknowledgedAt.HasValue
        ? AcknowledgedAt.Value - OccurredAt
        : null;

    public TimeSpan? TimeToResolve => ResolvedAt.HasValue
        ? ResolvedAt.Value - OccurredAt
        : null;
}
