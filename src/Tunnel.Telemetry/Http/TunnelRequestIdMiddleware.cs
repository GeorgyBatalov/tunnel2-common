using Microsoft.AspNetCore.Http;

namespace Tunnel.Telemetry.Http;

/// <summary>
/// Middleware that extracts or generates tunnelRequestId from X-Tunnel-Request-Id header.
/// </summary>
/// <remarks>
/// This middleware:
/// 1. Reads X-Tunnel-Request-Id from incoming request
/// 2. If missing, generates a new GUID
/// 3. Sets it in CorrelationContext (which auto-syncs to Activity.Current)
/// 4. Adds X-Tunnel-Request-Id to response headers
///
/// Should be one of the first middleware in the pipeline.
/// </remarks>
public sealed class TunnelRequestIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ICorrelationContextAccessor _accessor;
    private const string HeaderName = "X-Tunnel-Request-Id";

    public TunnelRequestIdMiddleware(
        RequestDelegate next,
        ICorrelationContextAccessor accessor)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _accessor = accessor ?? throw new ArgumentNullException(nameof(accessor));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Extract from header or generate new
        var requestId = context.Request.Headers[HeaderName].FirstOrDefault()
                        ?? Guid.NewGuid().ToString();

        // Set in correlation context (auto-syncs to Activity.Current)
        _accessor.Current.SetTunnelRequestId(requestId);

        // Add to response headers for traceability (immediately, not OnStarting)
        if (!context.Response.Headers.ContainsKey(HeaderName))
        {
            context.Response.Headers[HeaderName] = requestId;
        }

        await _next(context);
    }
}
