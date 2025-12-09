namespace SmartFactory.Infrastructure.OpcUa.Exceptions;

/// <summary>
/// Base exception for OPC-UA related errors.
/// </summary>
public class OpcUaException : Exception
{
    /// <summary>
    /// The OPC-UA node ID related to the error, if applicable.
    /// </summary>
    public string? NodeId { get; }

    /// <summary>
    /// The OPC-UA status code, if applicable.
    /// </summary>
    public uint StatusCode { get; }

    /// <summary>
    /// The name of the OPC-UA server where the error occurred.
    /// </summary>
    public string? ServerName { get; }

    public OpcUaException(string message)
        : base(message)
    {
    }

    public OpcUaException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    public OpcUaException(string? nodeId, uint statusCode, string? serverName, string message)
        : base(message)
    {
        NodeId = nodeId;
        StatusCode = statusCode;
        ServerName = serverName;
    }

    public OpcUaException(string? nodeId, uint statusCode, string? serverName, string message, Exception innerException)
        : base(message, innerException)
    {
        NodeId = nodeId;
        StatusCode = statusCode;
        ServerName = serverName;
    }
}

/// <summary>
/// Exception thrown when writing to an OPC-UA node fails.
/// </summary>
public class OpcUaWriteException : OpcUaException
{
    /// <summary>
    /// The value that was attempted to be written.
    /// </summary>
    public object? AttemptedValue { get; }

    public OpcUaWriteException(string nodeId, uint statusCode, string? serverName, object? attemptedValue)
        : base(nodeId, statusCode, serverName, $"Failed to write value to node '{nodeId}': OPC-UA status code {statusCode}")
    {
        AttemptedValue = attemptedValue;
    }

    public OpcUaWriteException(string nodeId, uint statusCode, string? serverName, object? attemptedValue, Exception innerException)
        : base(nodeId, statusCode, serverName, $"Failed to write value to node '{nodeId}': OPC-UA status code {statusCode}", innerException)
    {
        AttemptedValue = attemptedValue;
    }
}

/// <summary>
/// Exception thrown when reading from an OPC-UA node fails.
/// </summary>
public class OpcUaReadException : OpcUaException
{
    public OpcUaReadException(string nodeId, uint statusCode, string? serverName)
        : base(nodeId, statusCode, serverName, $"Failed to read value from node '{nodeId}': OPC-UA status code {statusCode}")
    {
    }

    public OpcUaReadException(string nodeId, uint statusCode, string? serverName, Exception innerException)
        : base(nodeId, statusCode, serverName, $"Failed to read value from node '{nodeId}': OPC-UA status code {statusCode}", innerException)
    {
    }
}

/// <summary>
/// Exception thrown when connecting to an OPC-UA server fails.
/// </summary>
public class OpcUaConnectionException : OpcUaException
{
    /// <summary>
    /// The endpoint URL that was attempted.
    /// </summary>
    public string? EndpointUrl { get; }

    public OpcUaConnectionException(string? serverName, string? endpointUrl, string message)
        : base(null, 0, serverName, message)
    {
        EndpointUrl = endpointUrl;
    }

    public OpcUaConnectionException(string? serverName, string? endpointUrl, string message, Exception innerException)
        : base(null, 0, serverName, message, innerException)
    {
        EndpointUrl = endpointUrl;
    }
}

/// <summary>
/// Exception thrown when an OPC-UA subscription operation fails.
/// </summary>
public class OpcUaSubscriptionException : OpcUaException
{
    /// <summary>
    /// The subscription ID, if applicable.
    /// </summary>
    public uint? SubscriptionId { get; }

    public OpcUaSubscriptionException(string? serverName, uint? subscriptionId, string message)
        : base(null, 0, serverName, message)
    {
        SubscriptionId = subscriptionId;
    }

    public OpcUaSubscriptionException(string? serverName, uint? subscriptionId, string message, Exception innerException)
        : base(null, 0, serverName, message, innerException)
    {
        SubscriptionId = subscriptionId;
    }
}
