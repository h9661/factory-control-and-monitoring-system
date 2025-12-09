namespace SmartFactory.Infrastructure.OpcUa.Security;

/// <summary>
/// Provides credentials for OPC-UA server authentication.
/// </summary>
public interface IOpcUaCredentialProvider
{
    /// <summary>
    /// Gets credentials for the specified server.
    /// </summary>
    /// <param name="serverId">The server identifier.</param>
    /// <returns>A tuple containing the username and password.</returns>
    Task<(string? Username, string? Password)> GetCredentialsAsync(string serverId);

    /// <summary>
    /// Indicates whether this provider uses secure storage.
    /// </summary>
    bool SupportsSecureStorage { get; }

    /// <summary>
    /// Gets the name of this credential provider for logging purposes.
    /// </summary>
    string ProviderName { get; }
}
