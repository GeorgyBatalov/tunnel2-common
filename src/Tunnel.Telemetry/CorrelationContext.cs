namespace Tunnel.Telemetry;

/// <summary>
/// Holds correlation identifiers for distributed tracing across Tunnel2 services.
/// All identifiers use the 'tunnel*' prefix for unified naming convention.
/// </summary>
public sealed class CorrelationContext
{
    /// <summary>
    /// HTTP request identifier (extracted from X-Tunnel-Request-Id header or generated).
    /// Scope: Single HTTP request (transient).
    /// </summary>
    public string? TunnelRequestId { get; set; }

    /// <summary>
    /// Client identifier (HardwareThumbprint / LicenseId).
    /// Scope: License/Hardware (persistent).
    /// </summary>
    public string? TunnelClientId { get; set; }

    /// <summary>
    /// Tunnel identifier (persistent across reconnects, Phase 7).
    /// Scope: Logical tunnel (persistent).
    /// </summary>
    public string? TunnelId { get; set; }

    /// <summary>
    /// ProxyEntry instance identifier (hostname or pod name).
    /// Scope: ProxyEntry deployment (per instance).
    /// Used for load balancing debugging and traffic distribution analysis.
    /// </summary>
    public string? TunnelProxyId { get; set; }

    /// <summary>
    /// TCP session identifier (ephemeral, optional).
    /// Scope: Single TCP connection.
    /// </summary>
    public string? TunnelSessionId { get; set; }

    /// <summary>
    /// Additional custom tags for specific scenarios (tier, region, etc.).
    /// Low cardinality recommended for metrics.
    /// </summary>
    public Dictionary<string, string> CustomTags { get; } = new();

    /// <summary>
    /// Clears all context fields (useful for testing or manual reset).
    /// </summary>
    public void Clear()
    {
        TunnelRequestId = null;
        TunnelClientId = null;
        TunnelId = null;
        TunnelProxyId = null;
        TunnelSessionId = null;
        CustomTags.Clear();
    }
}
