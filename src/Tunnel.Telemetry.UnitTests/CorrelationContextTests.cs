using System.Diagnostics;

namespace Tunnel.Telemetry.UnitTests;

public class CorrelationContextTests
{
    [Fact]
    public void SetTunnelId_SetsContextAndActivityTag()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelId("tunnel-123");

        // Assert
        Assert.Equal("tunnel-123", ctx.TunnelId);
        Assert.Equal("tunnel-123", activity.GetTagItem("tunnelId"));
    }

    [Fact]
    public void SetTunnelClientId_SetsContextAndActivityTag()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelClientId("client-456");

        // Assert
        Assert.Equal("client-456", ctx.TunnelClientId);
        Assert.Equal("client-456", activity.GetTagItem("tunnelClientId"));
    }

    [Fact]
    public void SetTunnelRequestId_SetsContextAndActivityTag()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelRequestId("req-789");

        // Assert
        Assert.Equal("req-789", ctx.TunnelRequestId);
        Assert.Equal("req-789", activity.GetTagItem("tunnelRequestId"));
    }

    [Fact]
    public void SetTunnelProxyId_SetsContextAndActivityTag()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelProxyId("proxy-1");

        // Assert
        Assert.Equal("proxy-1", ctx.TunnelProxyId);
        Assert.Equal("proxy-1", activity.GetTagItem("tunnelProxyId"));
    }

    [Fact]
    public void SetTunnelSessionId_SetsContextAndActivityTag()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelSessionId("sess-abc");

        // Assert
        Assert.Equal("sess-abc", ctx.TunnelSessionId);
        Assert.Equal("sess-abc", activity.GetTagItem("tunnelSessionId"));
    }

    [Fact]
    public void SetTunnelContext_SetsAllFields()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelContext(
            tunnelId: "tunnel-123",
            clientId: "client-456",
            proxyId: "proxy-1",
            sessionId: "sess-abc"
        );

        // Assert
        Assert.Equal("tunnel-123", ctx.TunnelId);
        Assert.Equal("client-456", ctx.TunnelClientId);
        Assert.Equal("proxy-1", ctx.TunnelProxyId);
        Assert.Equal("sess-abc", ctx.TunnelSessionId);

        Assert.Equal("tunnel-123", activity.GetTagItem("tunnelId"));
        Assert.Equal("client-456", activity.GetTagItem("tunnelClientId"));
        Assert.Equal("proxy-1", activity.GetTagItem("tunnelProxyId"));
        Assert.Equal("sess-abc", activity.GetTagItem("tunnelSessionId"));
    }

    [Fact]
    public void SetTunnelContext_WithOptionalParametersNull_OnlySetsRequiredFields()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.SetTunnelContext(
            tunnelId: "tunnel-123",
            clientId: "client-456"
        );

        // Assert
        Assert.Equal("tunnel-123", ctx.TunnelId);
        Assert.Equal("client-456", ctx.TunnelClientId);
        Assert.Null(ctx.TunnelProxyId);
        Assert.Null(ctx.TunnelSessionId);
    }

    [Fact]
    public void AddCustomTag_AddsToContextAndActivity()
    {
        // Arrange
        var ctx = new CorrelationContext();
        using var activity = new Activity("test").Start();

        // Act
        ctx.AddCustomTag("tier", "professional");

        // Assert
        Assert.True(ctx.CustomTags.ContainsKey("tier"));
        Assert.Equal("professional", ctx.CustomTags["tier"]);
        Assert.Equal("professional", activity.GetTagItem("tier"));
    }

    [Fact]
    public void Clear_ResetsAllFields()
    {
        // Arrange
        var ctx = new CorrelationContext();
        ctx.SetTunnelContext("tunnel-123", "client-456", "proxy-1", "sess-abc");
        ctx.AddCustomTag("tier", "professional");

        // Act
        ctx.Clear();

        // Assert
        Assert.Null(ctx.TunnelId);
        Assert.Null(ctx.TunnelClientId);
        Assert.Null(ctx.TunnelProxyId);
        Assert.Null(ctx.TunnelSessionId);
        Assert.Null(ctx.TunnelRequestId);
        Assert.Empty(ctx.CustomTags);
    }

    [Fact]
    public void SetMethods_WithNoActiveActivity_DoNotThrow()
    {
        // Arrange
        var ctx = new CorrelationContext();
        Assert.Null(Activity.Current);

        // Act & Assert - should not throw
        ctx.SetTunnelId("tunnel-123");
        ctx.SetTunnelClientId("client-456");
        ctx.SetTunnelRequestId("req-789");
        ctx.SetTunnelProxyId("proxy-1");
        ctx.SetTunnelSessionId("sess-abc");

        // Context should still be set
        Assert.Equal("tunnel-123", ctx.TunnelId);
        Assert.Equal("client-456", ctx.TunnelClientId);
        Assert.Equal("req-789", ctx.TunnelRequestId);
        Assert.Equal("proxy-1", ctx.TunnelProxyId);
        Assert.Equal("sess-abc", ctx.TunnelSessionId);
    }
}
