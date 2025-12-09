using Microsoft.Extensions.Logging;

namespace SmartFactory.Infrastructure.OpcUa.Security;

/// <summary>
/// Credential provider that reads credentials from environment variables.
/// This is the recommended approach for production deployments.
///
/// Environment variable naming convention:
/// - Username: OPCUA_{SERVERID}_USERNAME
/// - Password: OPCUA_{SERVERID}_PASSWORD
///
/// Where {SERVERID} is the server ID in uppercase with special characters replaced by underscores.
/// </summary>
public class EnvironmentCredentialProvider : IOpcUaCredentialProvider
{
    private readonly ILogger<EnvironmentCredentialProvider> _logger;

    public EnvironmentCredentialProvider(ILogger<EnvironmentCredentialProvider> logger)
    {
        _logger = logger;
    }

    public bool SupportsSecureStorage => true;
    public string ProviderName => "Environment";

    public Task<(string? Username, string? Password)> GetCredentialsAsync(string serverId)
    {
        var normalizedServerId = NormalizeServerId(serverId);

        var usernameKey = $"OPCUA_{normalizedServerId}_USERNAME";
        var passwordKey = $"OPCUA_{normalizedServerId}_PASSWORD";

        var username = Environment.GetEnvironmentVariable(usernameKey);
        var password = Environment.GetEnvironmentVariable(passwordKey);

        if (string.IsNullOrEmpty(username))
        {
            _logger.LogDebug(
                "No username found in environment variable {UsernameKey} for server {ServerId}",
                usernameKey, serverId);
        }

        return Task.FromResult<(string?, string?)>((username, password));
    }

    /// <summary>
    /// Normalizes the server ID for use in environment variable names.
    /// Converts to uppercase and replaces special characters with underscores.
    /// </summary>
    private static string NormalizeServerId(string serverId)
    {
        return serverId
            .ToUpperInvariant()
            .Replace("-", "_")
            .Replace(".", "_")
            .Replace(" ", "_");
    }
}
