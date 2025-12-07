namespace SmartFactory.Domain.Common;

/// <summary>
/// Base entity class with common properties for all domain entities.
/// </summary>
public abstract class BaseEntity
{
    public Guid Id { get; protected set; } = Guid.NewGuid();
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; protected set; }
    public string? CreatedBy { get; protected set; }
    public string? UpdatedBy { get; protected set; }

    public void SetCreatedBy(string userId)
    {
        CreatedBy = userId;
    }

    public void SetUpdatedBy(string userId)
    {
        UpdatedBy = userId;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Base entity with audit trail for soft delete support.
/// </summary>
public abstract class AuditableEntity : BaseEntity
{
    public bool IsDeleted { get; protected set; }
    public DateTime? DeletedAt { get; protected set; }
    public string? DeletedBy { get; protected set; }

    public void MarkAsDeleted(string userId)
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        DeletedBy = userId;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }
}
