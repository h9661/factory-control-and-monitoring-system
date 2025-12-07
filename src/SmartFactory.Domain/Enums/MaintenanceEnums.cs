namespace SmartFactory.Domain.Enums;

/// <summary>
/// Type of maintenance activity.
/// </summary>
public enum MaintenanceType
{
    /// <summary>Scheduled preventive maintenance.</summary>
    Preventive = 1,

    /// <summary>Corrective maintenance after failure.</summary>
    Corrective = 2,

    /// <summary>Predictive maintenance based on condition monitoring.</summary>
    Predictive = 3,

    /// <summary>Routine calibration.</summary>
    Calibration = 4,

    /// <summary>Emergency repair.</summary>
    Emergency = 5
}

/// <summary>
/// Status of a maintenance record.
/// </summary>
public enum MaintenanceStatus
{
    /// <summary>Maintenance is scheduled for future.</summary>
    Scheduled = 0,

    /// <summary>Maintenance is currently in progress.</summary>
    InProgress = 1,

    /// <summary>Maintenance has been completed.</summary>
    Completed = 2,

    /// <summary>Maintenance was cancelled.</summary>
    Cancelled = 3,

    /// <summary>Maintenance is overdue.</summary>
    Overdue = 4
}
