using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Tunnel.Telemetry.Http;

namespace Tunnel.Telemetry.DependencyInjection;

/// <summary>
/// Extension methods for configuring Tunnel telemetry services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Tunnel telemetry services including CorrelationContext, OpenTelemetry, and HTTP handlers.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="serviceName">Service name for OpenTelemetry (e.g., "TunnelServer", "ProxyEntry")</param>
    /// <param name="configureTracing">Optional configuration for OpenTelemetry tracing.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddTunnelTelemetry(
        this IServiceCollection services,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        if (string.IsNullOrWhiteSpace(serviceName))
        {
            throw new ArgumentException("Service name cannot be null or empty", nameof(serviceName));
        }

        // Register CorrelationContext as singleton
        services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();

        // Register HTTP correlation propagation handler as transient
        services.AddTransient<TunnelCorrelationPropagationHandler>();

        // Configure OpenTelemetry tracing
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: serviceName)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
                }))
            .WithTracing(tracerBuilder =>
            {
                // Add Tunnel2 activity sources
                tracerBuilder.AddSource("Tunnel2.*");

                // Phase 8 Week 3-4: Add custom ActivitySource names for all services
                tracerBuilder.AddSource("TunnelServer");
                tracerBuilder.AddSource("ProxyEntry");
                tracerBuilder.AddSource("TunnelClient"); // Phase 8 Week 3-4 Day 4

                // Add HttpClient instrumentation (auto-creates spans for outgoing HTTP calls)
                tracerBuilder.AddHttpClientInstrumentation(options =>
                {
                    // Enrich spans with correlation context
                    options.EnrichWithHttpRequestMessage = (activity, httpRequest) =>
                    {
                        // Tags will be set by TunnelCorrelationPropagationHandler via headers
                        // Just mark this as a tunnel service call
                        activity.SetTag("tunnel.outgoing_call", true);
                    };
                });

                // Apply custom configuration
                configureTracing?.Invoke(tracerBuilder);
            });

        return services;
    }

    /// <summary>
    /// Adds OTLP exporter for sending traces to OpenTelemetry collector or Jaeger.
    /// </summary>
    /// <param name="builder">The tracer provider builder.</param>
    /// <param name="endpoint">OTLP endpoint (default: http://localhost:4317)</param>
    /// <returns>The tracer provider builder for chaining.</returns>
    public static TracerProviderBuilder AddTunnelOtlpExporter(
        this TracerProviderBuilder builder,
        string? endpoint = null)
    {
        return builder.AddOtlpExporter(options =>
        {
            options.Endpoint = new Uri(endpoint ?? "http://localhost:4317");
            options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
        });
    }
}
