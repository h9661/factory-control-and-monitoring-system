namespace SmartFactory.Application.Exceptions;

/// <summary>
/// Exception thrown when an operation is not allowed due to business rules or state constraints.
/// </summary>
public class OperationNotAllowedException : SmartFactoryException
{
    /// <summary>
    /// The operation that was attempted.
    /// </summary>
    public string? Operation { get; }

    /// <summary>
    /// The reason why the operation is not allowed.
    /// </summary>
    public string? Reason { get; }

    public OperationNotAllowedException(string message)
        : base("OPERATION_NOT_ALLOWED", message)
    {
    }

    public OperationNotAllowedException(string operation, string reason)
        : base("OPERATION_NOT_ALLOWED", $"Operation '{operation}' is not allowed: {reason}")
    {
        Operation = operation;
        Reason = reason;
    }

    public OperationNotAllowedException(string message, Exception innerException)
        : base("OPERATION_NOT_ALLOWED", message, innerException)
    {
    }

    /// <summary>
    /// Creates an exception for invalid state transition.
    /// </summary>
    public static OperationNotAllowedException InvalidStateTransition(string entityType, string currentState, string targetState)
    {
        return new OperationNotAllowedException(
            "StateTransition",
            $"Cannot transition {entityType} from '{currentState}' to '{targetState}'"
        );
    }
}
