namespace SmartFactory.Domain.Enums;

/// <summary>
/// Result of a quality inspection.
/// </summary>
public enum InspectionResult
{
    /// <summary>Inspection passed all criteria.</summary>
    Pass = 0,

    /// <summary>Inspection failed one or more criteria.</summary>
    Fail = 1,

    /// <summary>Inspection passed with minor issues noted.</summary>
    ConditionalPass = 2,

    /// <summary>Inspection was skipped or not applicable.</summary>
    NotApplicable = 3
}

/// <summary>
/// Type of quality inspection.
/// </summary>
public enum InspectionType
{
    /// <summary>Visual inspection by operator.</summary>
    Visual = 1,

    /// <summary>Automated Optical Inspection.</summary>
    AOI = 2,

    /// <summary>X-ray inspection.</summary>
    XRay = 3,

    /// <summary>In-Circuit Test.</summary>
    ICT = 4,

    /// <summary>Functional test.</summary>
    Functional = 5,

    /// <summary>Final quality check before shipping.</summary>
    FinalQC = 6
}

/// <summary>
/// Common defect types in electronics manufacturing.
/// </summary>
public enum DefectType
{
    None = 0,
    SolderBridge = 1,
    InsufficientSolder = 2,
    ColdSolder = 3,
    MissingComponent = 4,
    MisalignedComponent = 5,
    WrongComponent = 6,
    ReversedPolarity = 7,
    Tombstoning = 8,
    SolderBall = 9,
    PCBDamage = 10,
    ContaminationOrDebris = 11,
    Other = 99
}
