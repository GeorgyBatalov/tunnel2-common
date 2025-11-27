using Microsoft.Extensions.DependencyInjection;
using Tunnel.Telemetry.DependencyInjection;
using Tunnel.Telemetry.Http;

namespace Tunnel.Telemetry.UnitTests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTunnelTelemetry_RegistersCorrelationContextAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTunnelTelemetry("TestService");
        var provider = services.BuildServiceProvider();

        // Assert
        var accessor = provider.GetService<ICorrelationContextAccessor>();
        Assert.NotNull(accessor);
        Assert.IsType<CorrelationContextAccessor>(accessor);
    }

    [Fact]
    public void AddTunnelTelemetry_RegistersCorrelationContextAccessorAsSingleton()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTunnelTelemetry("TestService");
        var provider = services.BuildServiceProvider();

        // Assert
        var accessor1 = provider.GetService<ICorrelationContextAccessor>();
        var accessor2 = provider.GetService<ICorrelationContextAccessor>();
        Assert.Same(accessor1, accessor2);
    }

    [Fact]
    public void AddTunnelTelemetry_RegistersTunnelCorrelationPropagationHandler()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddTunnelTelemetry("TestService");
        var provider = services.BuildServiceProvider();

        // Assert
        var handler = provider.GetService<TunnelCorrelationPropagationHandler>();
        Assert.NotNull(handler);
    }

    [Fact]
    public void AddTunnelTelemetry_ThrowsWhenServiceNameIsNull()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddTunnelTelemetry(null!));
    }

    [Fact]
    public void AddTunnelTelemetry_ThrowsWhenServiceNameIsEmpty()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddTunnelTelemetry(string.Empty));
    }

    [Fact]
    public void AddTunnelTelemetry_ThrowsWhenServiceNameIsWhitespace()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act & Assert
        Assert.Throws<ArgumentException>(() => services.AddTunnelTelemetry("   "));
    }

    [Fact]
    public void AddTunnelTelemetry_AllowsCustomTracingConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        var customConfigCalled = false;

        // Act
        services.AddTunnelTelemetry("TestService", builder =>
        {
            customConfigCalled = true;
        });

        var provider = services.BuildServiceProvider();

        // Assert
        Assert.True(customConfigCalled);
    }

    [Fact]
    public void AddTunnelTelemetry_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddTunnelTelemetry("TestService");

        // Assert
        Assert.Same(services, result);
    }
}
