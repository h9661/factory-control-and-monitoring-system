namespace SmartFactory.Infrastructure.OpcUa.Configuration;

/// <summary>
/// Certificate validation mode for OPC-UA connections.
/// </summary>
public enum CertificateValidationMode
{
    /// <summary>
    /// Strict validation - reject untrusted certificates.
    /// Use this in production environments.
    /// </summary>
    Strict,

    /// <summary>
    /// Allow self-signed certificates but verify other aspects.
    /// Use for development with test servers.
    /// </summary>
    AllowSelfSigned,

    /// <summary>
    /// Demo mode - accept all certificates without validation.
    /// WARNING: This is insecure and should only be used for demonstrations.
    /// </summary>
    Demo
}

/// <summary>
/// Configuration options for OPC-UA certificate validation.
/// </summary>
public class CertificateValidationOptions
{
    /// <summary>
    /// The certificate validation mode. Default: Demo for backwards compatibility.
    /// IMPORTANT: Set to Strict for production deployments.
    /// </summary>
    public CertificateValidationMode ValidationMode { get; set; } = CertificateValidationMode.Demo;

    /// <summary>
    /// Whether to allow untrusted certificates.
    /// This is derived from ValidationMode but can be explicitly overridden.
    /// </summary>
    public bool AllowUntrustedCertificates { get; set; } = true;

    /// <summary>
    /// Whether to add the application certificate to the trusted store.
    /// Default: true for Strict mode, false for Demo mode.
    /// </summary>
    public bool AddAppCertToTrustedStore { get; set; } = false;

    /// <summary>
    /// Path to the trusted certificate store.
    /// </summary>
    public string? TrustedCertificateStorePath { get; set; }

    /// <summary>
    /// Whether to log warnings when running in insecure modes.
    /// Default: true.
    /// </summary>
    public bool WarnOnInsecureMode { get; set; } = true;

    /// <summary>
    /// Determines if the current configuration is production-safe.
    /// </summary>
    public bool IsProductionSafe => ValidationMode == CertificateValidationMode.Strict;

    /// <summary>
    /// Gets the effective AllowUntrustedCertificates value based on ValidationMode.
    /// </summary>
    public bool GetEffectiveAllowUntrustedCertificates()
    {
        return ValidationMode switch
        {
            CertificateValidationMode.Strict => false,
            CertificateValidationMode.AllowSelfSigned => true, // For self-signed, we still need to allow untrusted
            CertificateValidationMode.Demo => true,
            _ => AllowUntrustedCertificates
        };
    }

    /// <summary>
    /// Gets the effective AddAppCertToTrustedStore value based on ValidationMode.
    /// </summary>
    public bool GetEffectiveAddAppCertToTrustedStore()
    {
        return ValidationMode switch
        {
            CertificateValidationMode.Strict => true,
            CertificateValidationMode.AllowSelfSigned => true,
            CertificateValidationMode.Demo => false,
            _ => AddAppCertToTrustedStore
        };
    }
}
