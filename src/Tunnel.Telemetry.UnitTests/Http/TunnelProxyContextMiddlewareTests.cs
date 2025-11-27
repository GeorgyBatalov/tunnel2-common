using Microsoft.AspNetCore.Http;
using Tunnel.Telemetry.Http;

namespace Tunnel.Telemetry.UnitTests.Http;

public class TunnelProxyContextMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_SetsTunnelProxyId()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        // Pre-create context by accessing it (AsyncLocal behavior)
        var ctx = accessor.Current;

        var middleware = new TunnelProxyContextMiddleware(
            next: (context) => Task.CompletedTask,
            accessor: accessor);

        var httpContext = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        Assert.NotNull(accessor.Current.TunnelProxyId);
        Assert.NotEmpty(accessor.Current.TunnelProxyId);
    }

    [Fact]
    public async Task InvokeAsync_ProxyIdMatchesHostname()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        // Pre-create context by accessing it (AsyncLocal behavior)
        var ctx = accessor.Current;

        var middleware = new TunnelProxyContextMiddleware(
            next: (context) => Task.CompletedTask,
            accessor: accessor);

        var httpContext = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(httpContext);

        // Assert
        var expectedProxyId = Environment.GetEnvironmentVariable("HOSTNAME")
                              ?? Environment.GetEnvironmentVariable("COMPUTERNAME")
                              ?? Environment.MachineName;
        Assert.Equal(expectedProxyId, accessor.Current.TunnelProxyId);
    }

    [Fact]
    public async Task InvokeAsync_CallsNext()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        var nextCalled = false;
        var middleware = new TunnelProxyContextMiddleware(
            next: (ctx) => { nextCalled = true; return Task.CompletedTask; },
            accessor: accessor);

        var context = new DefaultHttpContext();

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(nextCalled);
    }
}
