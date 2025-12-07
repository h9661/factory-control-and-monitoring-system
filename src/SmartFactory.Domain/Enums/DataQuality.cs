namespace SmartFactory.Domain.Enums;

/// <summary>
/// Quality indicator for sensor/OPC data.
/// </summary>
public enum DataQuality
{
    /// <summary>Data is valid and reliable.</summary>
    Good = 0,

    /// <summary>Data value is uncertain or questionable.</summary>
    Uncertain = 1,

    /// <summary>Data is invalid or communication error.</summary>
    Bad = 2
}

/// <summary>
/// Status of a production line.
/// </summary>
public enum ProductionLineStatus
{
    /// <summary>Line is not operational.</summary>
    Offline = 0,

    /// <summary>Line is ready but not running.</summary>
    Idle = 1,

    /// <summary>Line is actively producing.</summary>
    Running = 2,

    /// <summary>Line is under maintenance.</summary>
    Maintenance = 3,

    /// <summary>Line is being set up for new product.</summary>
    Changeover = 4
}
