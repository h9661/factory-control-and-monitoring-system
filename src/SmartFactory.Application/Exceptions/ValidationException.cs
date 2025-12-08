namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : SmartFactoryException
{
    /// <summary>
    /// Dictionary of validation errors keyed by property name.
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", "One or more validation errors occurred.")
    {
        Errors = errors;
    }

    public ValidationException(string propertyName, string errorMessage)
        : base("VALIDATION_FAILED", errorMessage)
    {
        Errors = new Dictionary<string, string[]>
        {
            { propertyName, new[] { errorMessage } }
        };
    }

    public ValidationException(string message, IDictionary<string, string[]> errors)
        : base("VALIDATION_FAILED", message)
    {
        Errors = errors;
    }

    /// <summary>
    /// Creates a ValidationException from FluentValidation result.
    /// </summary>
    public static ValidationException FromFluentValidation(FluentValidation.Results.ValidationResult result)
    {
        var errors = result.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return new ValidationException(errors);
    }
}
