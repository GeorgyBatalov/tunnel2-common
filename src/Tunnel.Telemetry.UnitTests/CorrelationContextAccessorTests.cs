namespace Tunnel.Telemetry.UnitTests;

public class CorrelationContextAccessorTests
{
    [Fact]
    public void Current_ReturnsNonNull()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var ctx = accessor.Current;

        // Assert
        Assert.NotNull(ctx);
    }

    [Fact]
    public void Current_ReturnsSameInstanceInSameAsyncFlow()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var ctx1 = accessor.Current;
        var ctx2 = accessor.Current;

        // Assert
        Assert.Same(ctx1, ctx2);
    }

    [Fact]
    public async Task Current_FlowsThroughTaskRun()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var ctx1 = accessor.Current;
        ctx1.TunnelId = "tunnel-123";

        CorrelationContext? ctx2 = null;
        await Task.Run(() =>
        {
            // AsyncLocal flows through Task.Run by default (this is desired behavior)
            ctx2 = accessor.Current;
        });

        // Assert
        Assert.NotNull(ctx2);
        Assert.Same(ctx1, ctx2); // AsyncLocal flows by design
        Assert.Equal("tunnel-123", ctx2.TunnelId);
    }

    [Fact]
    public async Task Current_FlowsThroughAwait()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var ctx1 = accessor.Current;
        ctx1.TunnelId = "tunnel-123";

        await Task.Delay(1);

        var ctx2 = accessor.Current;

        // Assert
        Assert.Same(ctx1, ctx2);
        Assert.Equal("tunnel-123", ctx2.TunnelId);
    }

    [Fact]
    public async Task Current_FlowsThroughMultipleAwaits()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var ctx = accessor.Current;
        ctx.TunnelId = "tunnel-123";

        await Task.Delay(1);
        await Task.Yield();
        await Task.CompletedTask;

        // Assert
        Assert.Equal("tunnel-123", accessor.Current.TunnelId);
    }

    [Fact]
    public void Current_LazyInitialization_CreatesInstanceOnFirstAccess()
    {
        // Arrange
        var accessor = new CorrelationContextAccessor();

        // Act
        var ctx = accessor.Current;

        // Assert
        Assert.NotNull(ctx);
        Assert.Null(ctx.TunnelId); // Should be empty/default
    }
}
