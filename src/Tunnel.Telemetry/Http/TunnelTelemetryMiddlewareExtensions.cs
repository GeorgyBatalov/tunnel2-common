using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Tunnel.Telemetry.Http;

/// <summary>
/// Extension methods for registering Tunnel.Telemetry middleware.
/// </summary>
public static class TunnelTelemetryMiddlewareExtensions
{
    /// <summary>
    /// Adds TunnelRequestIdMiddleware to the pipeline.
    /// Extracts or generates X-Tunnel-Request-Id for each request.
    /// </summary>
    /// <remarks>
    /// Should be called early in the pipeline, before any business logic.
    /// </remarks>
    public static IApplicationBuilder UseTunnelRequestId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TunnelRequestIdMiddleware>();
    }

    /// <summary>
    /// Adds TunnelProxyContextMiddleware to the pipeline.
    /// Sets tunnelProxyId to the current hostname/pod name.
    /// </summary>
    /// <remarks>
    /// Should be called after UseTunnelRequestId() in the pipeline.
    /// Only use in ProxyEntry services.
    /// </remarks>
    public static IApplicationBuilder UseTunnelProxyContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TunnelProxyContextMiddleware>();
    }

    /// <summary>
    /// Adds both TunnelRequestId and TunnelProxyContext middleware to the pipeline.
    /// </summary>
    /// <remarks>
    /// Convenience method for ProxyEntry services.
    /// Equivalent to calling UseTunnelRequestId() and UseTunnelProxyContext().
    /// </remarks>
    public static IApplicationBuilder UseTunnelTelemetry(this IApplicationBuilder app)
    {
        return app
            .UseTunnelRequestId()
            .UseTunnelProxyContext();
    }
}

/// <summary>
/// Extension methods for registering Tunnel.Telemetry HTTP handlers.
/// </summary>
public static class TunnelTelemetryHttpClientExtensions
{
    /// <summary>
    /// Adds TunnelCorrelationPropagationHandler to HttpClient pipeline.
    /// Automatically propagates tunnel* correlation context to outgoing requests.
    /// </summary>
    /// <example>
    /// <code>
    /// services.AddHttpClient("MyService")
    ///     .AddTunnelCorrelationPropagation();
    /// </code>
    /// </example>
    public static IHttpClientBuilder AddTunnelCorrelationPropagation(
        this IHttpClientBuilder builder)
    {
        return builder.AddHttpMessageHandler<TunnelCorrelationPropagationHandler>();
    }
}
