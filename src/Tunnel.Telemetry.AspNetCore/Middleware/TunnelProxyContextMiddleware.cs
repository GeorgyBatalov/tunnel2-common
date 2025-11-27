using Microsoft.AspNetCore.Http;

namespace Tunnel.Telemetry.Http;

/// <summary>
/// Middleware that sets tunnelProxyId to identify the ProxyEntry instance.
/// </summary>
/// <remarks>
/// This middleware sets tunnelProxyId to the machine hostname (or pod name in Kubernetes).
/// Useful for debugging load balancing and identifying which ProxyEntry handled the request.
///
/// Should run early in the pipeline, after TunnelRequestIdMiddleware.
/// </remarks>
public sealed class TunnelProxyContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationContextAccessor _accessor;
    private readonly string _proxyId;

    public TunnelProxyContextMiddleware(
        RequestDelegate next,
        ICorrelationContextAccessor accessor)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));

        // Get hostname (works in Docker, Kubernetes, and localhost)
        _proxyId = Environment.GetEnvironmentVariable("HOSTNAME")
                   ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
                   ?? Environment.MachineName;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Set tunnelProxyId in correlation context
        _accessor.Current.SetTunnelProxyId(_proxyId);

        await _next(context);
    }
}
