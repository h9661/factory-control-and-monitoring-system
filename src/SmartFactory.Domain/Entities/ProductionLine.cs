using SmartFactory.Domain.Common;
using SmartFactory.Domain.Enums;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a production line within a factory.
/// </summary>
public class ProductionLine : BaseEntity
{
    public Guid FactoryId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int Sequence { get; private set; }
    public ProductionLineStatus Status { get; private set; } = ProductionLineStatus.Offline;
    public int DesignedCapacity { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public Factory Factory { get; private set; } = null!;

    private readonly List<Equipment> _equipment = new();
    public IReadOnlyCollection<Equipment> Equipment => _equipment.AsReadOnly();

    // Required for EF Core
    private ProductionLine() { }

    public ProductionLine(Guid factoryId, string code, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        FactoryId = factoryId;
        Code = code.ToUpperInvariant();
        Name = name;
    }

    public void Update(string name, string? description, int capacity)
    {
        Name = name;
        Description = description;
        DesignedCapacity = capacity;
    }

    public void SetSequence(int sequence)
    {
        Sequence = sequence;
    }

    public void UpdateStatus(ProductionLineStatus status)
    {
        Status = status;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public Equipment AddEquipment(string code, string name, EquipmentType type)
    {
        var equipment = new Equipment(Id, code, name, type);
        _equipment.Add(equipment);
        return equipment;
    }
}
