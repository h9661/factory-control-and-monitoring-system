using SmartFactory.Domain.Common;

namespace SmartFactory.Domain.Entities;

/// <summary>
/// Represents a manufacturing factory/facility.
/// </summary>
public class Factory : AuditableEntity
{
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Location { get; private set; }
    public string? Address { get; private set; }
    public string TimeZone { get; private set; } = "UTC";
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }

    private readonly List<ProductionLine> _productionLines = new();
    public IReadOnlyCollection<ProductionLine> ProductionLines => _productionLines.AsReadOnly();

    private readonly List<WorkOrder> _workOrders = new();
    public IReadOnlyCollection<WorkOrder> WorkOrders => _workOrders.AsReadOnly();

    // Required for EF Core
    private Factory() { }

    public Factory(string code, string name, string? location = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Code = code.ToUpperInvariant();
        Name = name;
        Location = location;
    }

    public void Update(string name, string? location, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
        Location = location;
        Description = description;
    }

    public void SetTimeZone(string timeZone)
    {
        TimeZone = timeZone;
    }

    public void SetContactInfo(string? email, string? phone)
    {
        ContactEmail = email;
        ContactPhone = phone;
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public ProductionLine AddProductionLine(string code, string name)
    {
        var line = new ProductionLine(Id, code, name);
        line.SetSequence(_productionLines.Count + 1);
        _productionLines.Add(line);
        return line;
    }
}
