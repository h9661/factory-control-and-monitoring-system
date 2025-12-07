namespace SmartFactory.Domain.Enums;

/// <summary>
/// Types of equipment used in semiconductor/electronics manufacturing.
/// </summary>
public enum EquipmentType
{
    /// <summary>Surface Mount Technology machine for placing components.</summary>
    SMTMachine = 1,

    /// <summary>Automated Optical Inspection machine.</summary>
    AOIMachine = 2,

    /// <summary>Reflow oven for soldering.</summary>
    ReflowOven = 3,

    /// <summary>Wave soldering machine.</summary>
    WaveSolder = 4,

    /// <summary>Pick and place machine.</summary>
    PickAndPlace = 5,

    /// <summary>Solder paste printer/stencil printer.</summary>
    SolderPastePrinter = 6,

    /// <summary>Conveyor system.</summary>
    Conveyor = 7,

    /// <summary>Testing station for quality checks.</summary>
    TestStation = 8,

    /// <summary>Packaging and labeling machine.</summary>
    PackagingMachine = 9,

    /// <summary>PCB cleaning machine.</summary>
    CleaningMachine = 10,

    /// <summary>X-ray inspection machine.</summary>
    XRayInspection = 11,

    /// <summary>Laser marking machine.</summary>
    LaserMarker = 12,

    /// <summary>Depaneling machine.</summary>
    Depaneling = 13,

    /// <summary>Generic or other equipment type.</summary>
    Other = 99
}
