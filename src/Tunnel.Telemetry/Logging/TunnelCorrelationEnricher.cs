using Serilog.Core;
using Serilog.Events;

namespace Tunnel.Telemetry.Logging;

/// <summary>
/// Serilog enricher that automatically adds tunnel* context fields to every log event.
/// </summary>
/// <remarks>
/// This enricher reads from <see cref="ICorrelationContextAccessor"/> and adds properties:
/// - tunnelRequestId (if set)
/// - tunnelClientId (if set)
/// - tunnelId (if set)
/// - tunnelProxyId (if set)
/// - tunnelSessionId (if set)
/// - Custom tags (if any)
///
/// Register in Serilog configuration:
/// <code>
/// Log.Logger = new LoggerConfiguration()
///     .Enrich.With(new TunnelCorrelationEnricher(accessor))
///     .CreateLogger();
/// </code>
/// </remarks>
public sealed class TunnelCorrelationEnricher : ILogEventEnricher
{
    private readonly ICorrelationContextAccessor _accessor;

    /// <summary>
    /// Initializes a new instance of <see cref="TunnelCorrelationEnricher"/>.
    /// </summary>
    /// <param name="accessor">The correlation context accessor.</param>
    public TunnelCorrelationEnricher(ICorrelationContextAccessor accessor)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    /// <summary>
    /// Enriches the log event with tunnel* context fields.
    /// </summary>
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var ctx = _accessor.Current;

        // Add tunnelRequestId (HTTP only)
        if (!string.IsNullOrEmpty(ctx.TunnelRequestId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("tunnelRequestId", ctx.TunnelRequestId));
        }

        // Add tunnelClientId (always)
        if (!string.IsNullOrEmpty(ctx.TunnelClientId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("tunnelClientId", ctx.TunnelClientId));
        }

        // Add tunnelId (core identity)
        if (!string.IsNullOrEmpty(ctx.TunnelId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("tunnelId", ctx.TunnelId));
        }

        // Add tunnelProxyId (ProxyEntry instance)
        if (!string.IsNullOrEmpty(ctx.TunnelProxyId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("tunnelProxyId", ctx.TunnelProxyId));
        }

        // Add tunnelSessionId (TCP, optional)
        if (!string.IsNullOrEmpty(ctx.TunnelSessionId))
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty("tunnelSessionId", ctx.TunnelSessionId));
        }

        // Add custom tags (tier, region, etc.)
        foreach (var (key, value) in ctx.CustomTags)
        {
            logEvent.AddPropertyIfAbsent(
                propertyFactory.CreateProperty(key, value));
        }
    }
}
