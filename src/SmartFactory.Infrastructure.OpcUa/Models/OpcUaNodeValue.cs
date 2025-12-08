using Opc.Ua;

namespace SmartFactory.Infrastructure.OpcUa.Models;

/// <summary>
/// Represents a value read from or to be written to an OPC-UA node.
/// </summary>
public record OpcUaNodeValue
{
    /// <summary>
    /// The node ID of the value.
    /// </summary>
    public string NodeId { get; init; } = string.Empty;

    /// <summary>
    /// The display name of the node.
    /// </summary>
    public string DisplayName { get; init; } = string.Empty;

    /// <summary>
    /// The value of the node.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>
    /// The data type of the value.
    /// </summary>
    public string DataType { get; init; } = string.Empty;

    /// <summary>
    /// The timestamp when the value was sampled at the source.
    /// </summary>
    public DateTime SourceTimestamp { get; init; }

    /// <summary>
    /// The timestamp when the value was received by the server.
    /// </summary>
    public DateTime ServerTimestamp { get; init; }

    /// <summary>
    /// The status code of the value.
    /// </summary>
    public uint StatusCode { get; init; }

    /// <summary>
    /// Whether the value is good (valid).
    /// </summary>
    public bool IsGood { get; init; }

    /// <summary>
    /// The equipment ID associated with this value.
    /// </summary>
    public Guid? EquipmentId { get; init; }

    /// <summary>
    /// The sensor type for this value.
    /// </summary>
    public string? SensorType { get; init; }

    /// <summary>
    /// The unit of measurement.
    /// </summary>
    public string? Unit { get; init; }
}

/// <summary>
/// Represents a subscription to an OPC-UA node.
/// </summary>
public class OpcUaSubscriptionInfo
{
    /// <summary>
    /// The subscription ID.
    /// </summary>
    public uint SubscriptionId { get; set; }

    /// <summary>
    /// The server ID this subscription belongs to.
    /// </summary>
    public string ServerId { get; set; } = string.Empty;

    /// <summary>
    /// The monitored items in this subscription.
    /// </summary>
    public List<OpcUaMonitoredItemInfo> MonitoredItems { get; set; } = new();

    /// <summary>
    /// The publishing interval in milliseconds.
    /// </summary>
    public int PublishingIntervalMs { get; set; }

    /// <summary>
    /// Whether the subscription is active.
    /// </summary>
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a monitored item in an OPC-UA subscription.
/// </summary>
public class OpcUaMonitoredItemInfo
{
    /// <summary>
    /// The client handle for this item.
    /// </summary>
    public uint ClientHandle { get; set; }

    /// <summary>
    /// The node ID being monitored.
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// The display name of the node.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// The sampling interval in milliseconds.
    /// </summary>
    public int SamplingIntervalMs { get; set; }

    /// <summary>
    /// Associated equipment ID.
    /// </summary>
    public Guid? EquipmentId { get; set; }

    /// <summary>
    /// Sensor type for this item.
    /// </summary>
    public string? SensorType { get; set; }

    /// <summary>
    /// Unit of measurement.
    /// </summary>
    public string? Unit { get; set; }
}

/// <summary>
/// Connection status for an OPC-UA server.
/// </summary>
public record OpcUaConnectionStatus
{
    /// <summary>
    /// The server ID.
    /// </summary>
    public string ServerId { get; init; } = string.Empty;

    /// <summary>
    /// The server name.
    /// </summary>
    public string ServerName { get; init; } = string.Empty;

    /// <summary>
    /// The endpoint URL.
    /// </summary>
    public string EndpointUrl { get; init; } = string.Empty;

    /// <summary>
    /// Whether currently connected.
    /// </summary>
    public bool IsConnected { get; init; }

    /// <summary>
    /// The session state.
    /// </summary>
    public string State { get; init; } = string.Empty;

    /// <summary>
    /// Last connection time.
    /// </summary>
    public DateTime? LastConnectedAt { get; init; }

    /// <summary>
    /// Last disconnection time.
    /// </summary>
    public DateTime? LastDisconnectedAt { get; init; }

    /// <summary>
    /// Last error message if any.
    /// </summary>
    public string? LastError { get; init; }
}
