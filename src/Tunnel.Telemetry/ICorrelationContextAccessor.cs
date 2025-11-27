namespace Tunnel.Telemetry;

/// <summary>
/// Provides access to the current <see cref="CorrelationContext"/> for the async flow.
/// Implementation uses <see cref="AsyncLocal{T}"/> to maintain context across async boundaries.
/// </summary>
public interface ICorrelationContextAccessor
{
    /// <summary>
    /// Gets the current correlation context for the async flow.
    /// Never returns null - creates a new instance if not set.
    /// </summary>
    CorrelationContext Current { get; }
}
