using Microsoft.AspNetCore.Http;
using Tunnel.Telemetry.Http;

namespace Tunnel.Telemetry.UnitTests.Http;

public class TunnelRequestIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WhenHeaderPresent_UsesProvidedRequestId()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        // Pre-create context by accessing it (AsyncLocal behavior)
        var ctx = accessor.Current;

        var middleware = new TunnelRequestIdMiddleware(
            next: (context) => Task.CompletedTask,
            accessor: accessor);

        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["X-Tunnel-Request-Id"] = "existing-request-id-123";

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.Equal("existing-request-id-123", accessor.Current.TunnelRequestId);
        Assert.Equal("existing-request-id-123", httpContext.Response.Headers["X-Tunnel-Request-Id"]);
    }

    [Fact]
    public async Task InvokeAsync_WhenHeaderMissing_GeneratesNewRequestId()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        // Pre-create context by accessing it (AsyncLocal behavior)
        var ctx = accessor.Current;

        var middleware = new TunnelRequestIdMiddleware(
            next: (context) => Task.CompletedTask,
            accessor: accessor);

        var httpContext = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.NotNull(accessor.Current.TunnelRequestId);
        Assert.NotEmpty(accessor.Current.TunnelRequestId);
        Assert.True(Guid.TryParse(accessor.Current.TunnelRequestId, out _)); // Should be a valid GUID
        Assert.Equal(accessor.Current.TunnelRequestId, httpContext.Response.Headers["X-Tunnel-Request-Id"]);
    }

    [Fact]
    public async Task InvokeAsync_SetsResponseHeader()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        var middleware = new TunnelRequestIdMiddleware(
            next: (ctx) => Task.CompletedTask,
            accessor: accessor);

        var context = new DefaultHttpContext();
        context.Request.Headers["X-Tunnel-Request-Id"] = "test-request-id";

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(context.Response.Headers.ContainsKey("X-Tunnel-Request-Id"));
        Assert.Equal("test-request-id", context.Response.Headers["X-Tunnel-Request-Id"]);
    }

    [Fact]
    public async Task InvokeAsync_CallsNext()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        var nextCalled = false;
        var middleware = new TunnelRequestIdMiddleware(
            next: (ctx) => { nextCalled = true; return Task.CompletedTask; },
            accessor: accessor);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
