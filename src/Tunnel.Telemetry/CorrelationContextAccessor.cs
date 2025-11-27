namespace Tunnel.Telemetry;

/// <summary>
/// Default implementation of <see cref="ICorrelationContextAccessor"/> using <see cref="AsyncLocal{T}"/>.
/// Thread-safe and async/await safe - context flows through async calls automatically.
/// </summary>
/// <remarks>
/// Benefits of AsyncLocal:
/// - Thread-safe: Each async flow has its own context
/// - Async/await safe: Context flows through Task continuations
/// - Zero overhead: No locks, no DI scope tracking
/// - Works everywhere: HTTP, TCP, background jobs
///
/// Register as Singleton in DI container.
/// </remarks>
public sealed class CorrelationContextAccessor : ICorrelationContextAccessor
{
    private static readonly AsyncLocal<CorrelationContext> _asyncLocal = new();

    /// <summary>
    /// Gets the current correlation context for the async flow.
    /// Creates a new instance if not set (lazy initialization).
    /// </summary>
    public CorrelationContext Current => _asyncLocal.Value ??= new CorrelationContext();
}
