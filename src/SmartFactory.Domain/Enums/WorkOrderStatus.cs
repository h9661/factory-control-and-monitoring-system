namespace SmartFactory.Domain.Enums;

/// <summary>
/// Status of a work order in the production system.
/// </summary>
public enum WorkOrderStatus
{
    /// <summary>Work order is being prepared, not yet scheduled.</summary>
    Draft = 0,

    /// <summary>Work order is scheduled for production.</summary>
    Scheduled = 1,

    /// <summary>Work order is currently being processed.</summary>
    InProgress = 2,

    /// <summary>Work order is temporarily paused.</summary>
    Paused = 3,

    /// <summary>Work order has been completed successfully.</summary>
    Completed = 4,

    /// <summary>Work order has been cancelled.</summary>
    Cancelled = 5,

    /// <summary>Work order is on hold pending resolution.</summary>
    OnHold = 6
}

/// <summary>
/// Priority levels for work orders.
/// </summary>
public enum WorkOrderPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3,
    Critical = 4
}

/// <summary>
/// Status of a work order step.
/// </summary>
public enum WorkOrderStepStatus
{
    /// <summary>Step is pending execution.</summary>
    Pending = 0,

    /// <summary>Step is currently in progress.</summary>
    InProgress = 1,

    /// <summary>Step has been completed successfully.</summary>
    Completed = 2,

    /// <summary>Step was skipped.</summary>
    Skipped = 3,

    /// <summary>Step failed and needs attention.</summary>
    Failed = 4
}
