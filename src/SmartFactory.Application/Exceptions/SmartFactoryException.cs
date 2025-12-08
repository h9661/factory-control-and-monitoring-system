namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Base exception for all Smart Factory application exceptions.
/// </summary>
public class SmartFactoryException : Exception
{
    /// <summary>
    /// Error code for categorizing the exception.
    /// </summary>
    public string Code { get; }

    /// <summary>
    /// Additional details about the exception.
    /// </summary>
    public IDictionary<string, object>? Details { get; }

    public SmartFactoryException(string code, string message, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
    }

    public SmartFactoryException(string code, string message, IDictionary<string, object> details, Exception? innerException = null)
        : base(message, innerException)
    {
        Code = code;
        Details = details;
    }
}
