namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when attempting to create an entity that already exists.
/// </summary>
public class DuplicateEntityException : SmartFactoryException
{
    /// <summary>
    /// The type of entity that was duplicated.
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// The property that caused the duplicate violation.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// The value that was duplicated.
    /// </summary>
    public object PropertyValue { get; }

    public DuplicateEntityException(string entityType, string propertyName, object propertyValue)
        : base("DUPLICATE_ENTITY", $"A {entityType} with {propertyName} '{propertyValue}' already exists.")
    {
        EntityType = entityType;
        PropertyName = propertyName;
        PropertyValue = propertyValue;
    }

    /// <summary>
    /// Creates a DuplicateEntityException for a specific entity type.
    /// </summary>
    public static DuplicateEntityException For<TEntity>(string propertyName, object propertyValue) where TEntity : class
    {
        return new DuplicateEntityException(typeof(TEntity).Name, propertyName, propertyValue);
    }
}
