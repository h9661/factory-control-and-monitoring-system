using System.Windows;

namespace SmartFactory.Presentation.Models;

/// <summary>
/// Represents the floor plan layout with equipment positions.
/// </summary>
public class FloorPlanLayout
{
    /// <summary>
    /// Equipment positions mapped by equipment ID.
    /// </summary>
    public Dictionary<Guid, EquipmentPosition> EquipmentPositions { get; set; } = new();

    /// <summary>
    /// Production line connections between equipment.
    /// </summary>
    public List<ProductionLineConnection> ProductionLineConnections { get; set; } = new();

    /// <summary>
    /// Layout width in pixels.
    /// </summary>
    public double LayoutWidth { get; set; } = 1200;

    /// <summary>
    /// Layout height in pixels.
    /// </summary>
    public double LayoutHeight { get; set; } = 800;

    /// <summary>
    /// Factory areas/zones.
    /// </summary>
    public List<FactoryZone> Zones { get; set; } = new();
}

/// <summary>
/// Represents an equipment position on the floor plan.
/// </summary>
public class EquipmentPosition
{
    /// <summary>
    /// Equipment ID.
    /// </summary>
    public Guid EquipmentId { get; set; }

    /// <summary>
    /// X coordinate on the floor plan.
    /// </summary>
    public double X { get; set; }

    /// <summary>
    /// Y coordinate on the floor plan.
    /// </summary>
    public double Y { get; set; }

    /// <summary>
    /// Width of the equipment icon.
    /// </summary>
    public double Width { get; set; } = 80;

    /// <summary>
    /// Height of the equipment icon.
    /// </summary>
    public double Height { get; set; } = 80;

    /// <summary>
    /// Rotation angle in degrees.
    /// </summary>
    public double Rotation { get; set; } = 0;
}

/// <summary>
/// Represents a connection between two pieces of equipment.
/// </summary>
public class ProductionLineConnection
{
    /// <summary>
    /// Source equipment ID.
    /// </summary>
    public Guid SourceEquipmentId { get; set; }

    /// <summary>
    /// Target equipment ID.
    /// </summary>
    public Guid TargetEquipmentId { get; set; }

    /// <summary>
    /// Connection type.
    /// </summary>
    public ConnectionType Type { get; set; } = ConnectionType.Flow;

    /// <summary>
    /// Whether the connection is currently active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Connection type between equipment.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// Material flow connection.
    /// </summary>
    Flow,

    /// <summary>
    /// Conveyor belt connection.
    /// </summary>
    Conveyor,

    /// <summary>
    /// Data/signal connection.
    /// </summary>
    Signal
}

/// <summary>
/// Represents a zone/area on the factory floor.
/// </summary>
public class FactoryZone
{
    /// <summary>
    /// Zone name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Zone boundary rectangle.
    /// </summary>
    public Rect Bounds { get; set; }

    /// <summary>
    /// Zone color (hex).
    /// </summary>
    public string Color { get; set; } = "#2D2D30";
}
