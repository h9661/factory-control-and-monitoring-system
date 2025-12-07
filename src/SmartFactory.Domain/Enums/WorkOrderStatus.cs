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
