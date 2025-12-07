namespace SmartFactory.Domain.Enums;

/// <summary>
/// Represents the operational status of equipment.
/// </summary>
public enum EquipmentStatus
{
    /// <summary>Equipment is not connected or powered off.</summary>
    Offline = 0,

    /// <summary>Equipment is connected but not actively processing.</summary>
    Idle = 1,

    /// <summary>Equipment is actively running and processing.</summary>
    Running = 2,

    /// <summary>Equipment has a warning condition but still operational.</summary>
    Warning = 3,

    /// <summary>Equipment has an error and requires attention.</summary>
    Error = 4,

    /// <summary>Equipment is under scheduled maintenance.</summary>
    Maintenance = 5,

    /// <summary>Equipment is in setup or changeover mode.</summary>
    Setup = 6
}
