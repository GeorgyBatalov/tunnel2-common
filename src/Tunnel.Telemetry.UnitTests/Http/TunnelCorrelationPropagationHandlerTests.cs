using Tunnel.Telemetry.Http;

namespace Tunnel.Telemetry.UnitTests.Http;

public class TunnelCorrelationPropagationHandlerTests
{
    [Fact]
    public async Task SendAsync_PropagatesAllTunnelFields()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        accessor.Current.SetTunnelContext(
            tunnelId: "tunnel-123",
            clientId: "client-456",
            proxyId: "proxy-1",
            sessionId: "session-abc");
        accessor.Current.SetTunnelRequestId("request-789");

        var handler = new TunnelCorrelationPropagationHandler(accessor)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.Equal("request-789", request.Headers.GetValues("X-Tunnel-Request-Id").FirstOrDefault());
        Assert.Equal("client-456", request.Headers.GetValues("X-Tunnel-Client-Id").FirstOrDefault());
        Assert.Equal("tunnel-123", request.Headers.GetValues("X-Tunnel-Id").FirstOrDefault());
        Assert.Equal("proxy-1", request.Headers.GetValues("X-Tunnel-Proxy-Id").FirstOrDefault());
        Assert.Equal("session-abc", request.Headers.GetValues("X-Tunnel-Session-Id").FirstOrDefault());
    }

    [Fact]
    public async Task SendAsync_OnlyPropagatesSetFields()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        accessor.Current.SetTunnelRequestId("request-123");
        accessor.Current.SetTunnelClientId("client-456");
        // tunnelId, proxyId, sessionId not set

        var handler = new TunnelCorrelationPropagationHandler(accessor)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.True(request.Headers.Contains("X-Tunnel-Request-Id"));
        Assert.True(request.Headers.Contains("X-Tunnel-Client-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Proxy-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Session-Id"));
    }

    [Fact]
    public async Task SendAsync_WithEmptyContext_DoesNotAddHeaders()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();
        // Context is empty

        var handler = new TunnelCorrelationPropagationHandler(accessor)
        {
            InnerHandler = new TestHttpMessageHandler()
        };

        var client = new HttpClient(handler);
        var request = new HttpRequestMessage(HttpMethod.Get, "http://example.com");

        // Act
        await client.SendAsync(request);

        // Assert
        Assert.False(request.Headers.Contains("X-Tunnel-Request-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Client-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Proxy-Id"));
        Assert.False(request.Headers.Contains("X-Tunnel-Session-Id"));
    }

    // Test helper
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
        }
    }
}
