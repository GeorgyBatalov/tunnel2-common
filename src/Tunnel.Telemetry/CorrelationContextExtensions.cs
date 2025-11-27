using System.Diagnostics;

namespace Tunnel.Telemetry;

/// <summary>
/// Extension methods for <see cref="CorrelationContext"/> that automatically synchronize
/// with <see cref="Activity.Current"/> (OpenTelemetry spans).
/// </summary>
/// <remarks>
/// Key principle: ONE call to Set*() updates BOTH:
/// 1. CorrelationContext (for Serilog enricher)
/// 2. Activity.Current (for OpenTelemetry traces)
///
/// This eliminates boilerplate and ensures consistency.
/// </remarks>
public static class CorrelationContextExtensions
{
    /// <summary>
    /// Sets the HTTP request identifier and syncs with Activity.Current.
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="requestId">The request ID (from X-Tunnel-Request-Id or generated).</param>
    public static void SetTunnelRequestId(this CorrelationContext ctx, string requestId)
    {
        ctx.TunnelRequestId = requestId;
        Activity.Current?.SetTag("tunnelRequestId", requestId);
    }

    /// <summary>
    /// Sets the client identifier and syncs with Activity.Current.
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="clientId">The client ID (HardwareThumbprint / LicenseId).</param>
    public static void SetTunnelClientId(this CorrelationContext ctx, string clientId)
    {
        ctx.TunnelClientId = clientId;
        Activity.Current?.SetTag("tunnelClientId", clientId);
    }

    /// <summary>
    /// Sets the tunnel identifier and syncs with Activity.Current.
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="tunnelId">The tunnel ID (persistent, Phase 7).</param>
    public static void SetTunnelId(this CorrelationContext ctx, string tunnelId)
    {
        ctx.TunnelId = tunnelId;
        Activity.Current?.SetTag("tunnelId", tunnelId);
    }

    /// <summary>
    /// Sets the ProxyEntry instance identifier and syncs with Activity.Current.
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="proxyId">The ProxyEntry ID (hostname or pod name).</param>
    public static void SetTunnelProxyId(this CorrelationContext ctx, string proxyId)
    {
        ctx.TunnelProxyId = proxyId;
        Activity.Current?.SetTag("tunnelProxyId", proxyId);
    }

    /// <summary>
    /// Sets the TCP session identifier and syncs with Activity.Current.
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="sessionId">The session ID (ephemeral, TCP).</param>
    public static void SetTunnelSessionId(this CorrelationContext ctx, string sessionId)
    {
        ctx.TunnelSessionId = sessionId;
        Activity.Current?.SetTag("tunnelSessionId", sessionId);
    }

    /// <summary>
    /// Sets all tunnel context fields at once (useful for handshake scenarios).
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="tunnelId">The tunnel ID (required).</param>
    /// <param name="clientId">The client ID (required).</param>
    /// <param name="proxyId">The ProxyEntry ID (optional).</param>
    /// <param name="sessionId">The session ID (optional).</param>
    public static void SetTunnelContext(
        this CorrelationContext ctx,
        string tunnelId,
        string clientId,
        string? proxyId = null,
        string? sessionId = null)
    {
        ctx.SetTunnelId(tunnelId);
        ctx.SetTunnelClientId(clientId);

        if (proxyId != null)
            ctx.SetTunnelProxyId(proxyId);

        if (sessionId != null)
            ctx.SetTunnelSessionId(sessionId);
    }

    /// <summary>
    /// Adds a custom tag to both CorrelationContext and Activity.Current.
    /// </summary>
    /// <param name="ctx">The correlation context.</param>
    /// <param name="key">The tag key (e.g., "tier", "region").</param>
    /// <param name="value">The tag value.</param>
    /// <remarks>
    /// Use sparingly for metrics - high cardinality keys (like IDs) should use the dedicated Set* methods.
    /// </remarks>
    public static void AddCustomTag(this CorrelationContext ctx, string key, string value)
    {
        ctx.CustomTags[key] = value;
        Activity.Current?.SetTag(key, value);
    }
}
