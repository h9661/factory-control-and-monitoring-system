namespace SmartFactory.Infrastructure.OpcUa.Configuration;

/// <summary>
/// Configuration options for OPC-UA connections.
/// </summary>
public class OpcUaOptions
{
    public const string SectionName = "OpcUa";

    /// <summary>
    /// List of OPC-UA server endpoints to connect to.
    /// </summary>
    public List<OpcUaServerConfig> Servers { get; set; } = new();

    /// <summary>
    /// Application name used in the OPC-UA client certificate.
    /// </summary>
    public string ApplicationName { get; set; } = "SmartFactory.OpcUaClient";

    /// <summary>
    /// Application URI used in the OPC-UA client certificate.
    /// </summary>
    public string ApplicationUri { get; set; } = "urn:SmartFactory:OpcUaClient";

    /// <summary>
    /// Product URI for the OPC-UA client.
    /// </summary>
    public string ProductUri { get; set; } = "urn:SmartFactory:OpcUaClient";

    /// <summary>
    /// Session timeout in milliseconds.
    /// </summary>
    public int SessionTimeoutMs { get; set; } = 60000;

    /// <summary>
    /// Subscription publishing interval in milliseconds.
    /// </summary>
    public int PublishingIntervalMs { get; set; } = 1000;

    /// <summary>
    /// Default sampling interval in milliseconds.
    /// </summary>
    public int SamplingIntervalMs { get; set; } = 500;

    /// <summary>
    /// Whether to automatically reconnect on connection loss.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Reconnect interval in milliseconds.
    /// </summary>
    public int ReconnectIntervalMs { get; set; } = 5000;
}

/// <summary>
/// Configuration for a single OPC-UA server.
/// </summary>
public class OpcUaServerConfig
{
    /// <summary>
    /// Unique identifier for this server configuration.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this server.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// OPC-UA endpoint URL (e.g., opc.tcp://localhost:4840).
    /// </summary>
    public string EndpointUrl { get; set; } = string.Empty;

    /// <summary>
    /// Security policy (None, Basic128Rsa15, Basic256, Basic256Sha256).
    /// </summary>
    public string SecurityPolicy { get; set; } = "None";

    /// <summary>
    /// Security mode (None, Sign, SignAndEncrypt).
    /// </summary>
    public string SecurityMode { get; set; } = "None";

    /// <summary>
    /// User identity type (Anonymous, UserName, Certificate).
    /// </summary>
    public string UserIdentityType { get; set; } = "Anonymous";

    /// <summary>
    /// Username for UserName authentication.
    /// </summary>
    public string? Username { get; set; }

    /// <summary>
    /// Password for UserName authentication.
    /// </summary>
    public string? Password { get; set; }

    /// <summary>
    /// Whether this server should be automatically connected on startup.
    /// </summary>
    public bool AutoConnect { get; set; } = false;

    /// <summary>
    /// Factory ID this server is associated with.
    /// </summary>
    public Guid? FactoryId { get; set; }

    /// <summary>
    /// List of nodes to subscribe to.
    /// </summary>
    public List<OpcUaNodeConfig> Nodes { get; set; } = new();
}

/// <summary>
/// Configuration for an OPC-UA node to monitor.
/// </summary>
public class OpcUaNodeConfig
{
    /// <summary>
    /// Node ID in OPC-UA format (e.g., ns=2;s=MyNode).
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// Display name for this node.
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Equipment ID this node is associated with.
    /// </summary>
    public Guid? EquipmentId { get; set; }

    /// <summary>
    /// Sensor type for this node (Temperature, Pressure, Vibration, etc.).
    /// </summary>
    public string? SensorType { get; set; }

    /// <summary>
    /// Unit of measurement.
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// Sampling interval override in milliseconds.
    /// </summary>
    public int? SamplingIntervalMs { get; set; }
}
