namespace SmartFactory.Application.DTOs.Factory;

/// <summary>
/// Data transfer object for factory list display.
/// </summary>
public record FactoryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public bool IsActive { get; init; }
    public int ProductionLineCount { get; init; }
    public int EquipmentCount { get; init; }
}

/// <summary>
/// Detailed factory information.
/// </summary>
public record FactoryDetailDto : FactoryDto
{
    public string? Address { get; init; }
    public string? Description { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IEnumerable<ProductionLineDto> ProductionLines { get; init; } = Enumerable.Empty<ProductionLineDto>();
}

/// <summary>
/// Production line DTO.
/// </summary>
public record ProductionLineDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public string Status { get; init; } = string.Empty;
    public int DesignedCapacity { get; init; }
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public int EquipmentCount { get; init; }
}

/// <summary>
/// DTO for creating new factory.
/// </summary>
public record FactoryCreateDto
{
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? Address { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public string? Description { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
}

/// <summary>
/// DTO for updating factory.
/// </summary>
public record FactoryUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public string? Location { get; init; }
    public string? Address { get; init; }
    public string TimeZone { get; init; } = "UTC";
    public bool IsActive { get; init; }
    public string? Description { get; init; }
    public string? ContactEmail { get; init; }
    public string? ContactPhone { get; init; }
}

/// <summary>
/// DTO for creating new production line.
/// </summary>
public record ProductionLineCreateDto
{
    public Guid FactoryId { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public string? Description { get; init; }
    public int DesignedCapacity { get; init; }
}

/// <summary>
/// DTO for updating production line.
/// </summary>
public record ProductionLineUpdateDto
{
    public string Name { get; init; } = string.Empty;
    public int Sequence { get; init; }
    public bool IsActive { get; init; }
    public string? Description { get; init; }
    public int DesignedCapacity { get; init; }
}
