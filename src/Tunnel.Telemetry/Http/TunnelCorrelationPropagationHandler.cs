namespace Tunnel.Telemetry.Http;

/// <summary>
/// HTTP message handler that propagates tunnel* correlation context to outgoing HTTP requests.
/// </summary>
/// <remarks>
/// This handler automatically adds X-Tunnel-* headers to all outgoing HTTP requests:
/// - X-Tunnel-Request-Id (if set)
/// - X-Tunnel-Client-Id (if set)
/// - X-Tunnel-Id (if set)
/// - X-Tunnel-Proxy-Id (if set)
/// - X-Tunnel-Session-Id (if set)
///
/// Register with HttpClient:
/// <code>
/// services.AddHttpClient("MyService")
///     .AddHttpMessageHandler&lt;TunnelCorrelationPropagationHandler&gt;();
/// </code>
/// </remarks>
public sealed class TunnelCorrelationPropagationHandler : DelegatingHandler
{
    private readonly ICorrelationContextAccessor _accessor;

    public TunnelCorrelationPropagationHandler(ICorrelationContextAccessor accessor)
    {
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var context = _accessor.Current;

        // Propagate all tunnel* fields as headers
        if (!string.IsNullOrEmpty(context.TunnelRequestId))
        {
            request.Headers.TryAddWithoutValidation("X-Tunnel-Request-Id", context.TunnelRequestId);
        }

        if (!string.IsNullOrEmpty(context.TunnelClientId))
        {
            request.Headers.TryAddWithoutValidation("X-Tunnel-Client-Id", context.TunnelClientId);
        }

        if (!string.IsNullOrEmpty(context.TunnelId))
        {
            request.Headers.TryAddWithoutValidation("X-Tunnel-Id", context.TunnelId);
        }

        if (!string.IsNullOrEmpty(context.TunnelProxyId))
        {
            request.Headers.TryAddWithoutValidation("X-Tunnel-Proxy-Id", context.TunnelProxyId);
        }

        if (!string.IsNullOrEmpty(context.TunnelSessionId))
        {
            request.Headers.TryAddWithoutValidation("X-Tunnel-Session-Id", context.TunnelSessionId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
