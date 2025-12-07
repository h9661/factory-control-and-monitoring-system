namespace SmartFactory.Domain.Enums;

/// <summary>
/// Severity levels for alarms.
/// </summary>
public enum AlarmSeverity
{
    /// <summary>Informational message, no action required.</summary>
    Information = 0,

    /// <summary>Warning condition, monitoring recommended.</summary>
    Warning = 1,

    /// <summary>Error condition, action required.</summary>
    Error = 2,

    /// <summary>Critical condition, immediate action required.</summary>
    Critical = 3
}

/// <summary>
/// Status of an alarm.
/// </summary>
public enum AlarmStatus
{
    /// <summary>Alarm is currently active.</summary>
    Active = 0,

    /// <summary>Alarm has been acknowledged by operator.</summary>
    Acknowledged = 1,

    /// <summary>Alarm has been resolved.</summary>
    Resolved = 2
}
