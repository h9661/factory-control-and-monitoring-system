namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when a requested entity is not found.
/// </summary>
public class NotFoundException : SmartFactoryException
{
    /// <summary>
    /// The type of entity that was not found.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The identifier used to search for the entity.
    /// </summary>
    public object EntityId { get; }

    public NotFoundException(string entityType, object entityId)
        : base("NOT_FOUND", $"{entityType} with identifier '{entityId}' was not found.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    public NotFoundException(string entityType, object entityId, string message)
        : base("NOT_FOUND", message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Creates a NotFoundException for a specific entity type.
    /// </summary>
    public static NotFoundException For<TEntity>(object id) where TEntity : class
    {
        return new NotFoundException(typeof(TEntity).Name, id);
    }
}
