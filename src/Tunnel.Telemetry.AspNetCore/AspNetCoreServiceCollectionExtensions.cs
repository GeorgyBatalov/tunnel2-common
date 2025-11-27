using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Trace;

namespace Tunnel.Telemetry.AspNetCore;

/// <summary>
/// ASP.NET Core-specific extensions for Tunnel.Telemetry
/// </summary>
public static class AspNetCoreServiceCollectionExtensions
{
    /// <summary>
    /// Adds ASP.NET Core instrumentation to existing Tunnel Telemetry configuration.
    /// Must be called AFTER AddTunnelTelemetry().
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddTunnelTelemetryAspNetCore(this IServiceCollection services)
    {
        services.ConfigureOpenTelemetryTracerProvider(tracerBuilder =>
        {
            // Add ASP.NET Core instrumentation (auto-creates spans for HTTP requests)
            tracerBuilder.AddAspNetCoreInstrumentation(options =>
            {
                // Enrich spans with correlation context
                options.EnrichWithHttpRequest = (activity, httpRequest) =>
                {
                    var accessor = httpRequest.HttpContext.RequestServices
                        .GetService<ICorrelationContextAccessor>();
                    if (accessor != null)
                    {
                        var context = accessor.Current;
                        if (!string.IsNullOrEmpty(context.TunnelRequestId))
                            activity.SetTag("tunnel.request_id", context.TunnelRequestId);
                        if (!string.IsNullOrEmpty(context.TunnelClientId))
                            activity.SetTag("tunnel.client_id", context.TunnelClientId);
                        if (!string.IsNullOrEmpty(context.TunnelId))
                            activity.SetTag("tunnel.id", context.TunnelId);
                        if (!string.IsNullOrEmpty(context.TunnelProxyId))
                            activity.SetTag("tunnel.proxy_id", context.TunnelProxyId);
                        if (!string.IsNullOrEmpty(context.TunnelSessionId))
                            activity.SetTag("tunnel.session_id", context.TunnelSessionId);
                    }
                };
            });
        });

        return services;
    }
}
