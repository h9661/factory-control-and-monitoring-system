namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when a concurrency conflict occurs.
/// </summary>
public class ConcurrencyException : SmartFactoryException
{
    /// <summary>
    /// The type of entity that had the conflict.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The identifier of the entity.
    /// </summary>
    public object? EntityId { get; }

    public ConcurrencyException(string entityType)
        : base("CONCURRENCY_CONFLICT", $"The {entityType} was modified by another user. Please refresh and try again.")
    {
        EntityType = entityType;
    }

    public ConcurrencyException(string entityType, object entityId)
        : base("CONCURRENCY_CONFLICT", $"The {entityType} with id '{entityId}' was modified by another user. Please refresh and try again.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public ConcurrencyException(string entityType, object entityId, Exception innerException)
        : base("CONCURRENCY_CONFLICT", $"The {entityType} with id '{entityId}' was modified by another user.", innerException)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Creates a ConcurrencyException for a specific entity type.
    /// </summary>
    public static ConcurrencyException For<TEntity>(object? id = null) where TEntity : class
    {
        return id != null
            ? new ConcurrencyException(typeof(TEntity).Name, id)
            : new ConcurrencyException(typeof(TEntity).Name);
    }
}
