# Tunnel2 Common Libraries

Shared libraries and common infrastructure for Tunnel2 services.

## Libraries

### Tunnel.Telemetry

Distributed tracing and correlation context library for Tunnel2 services.

## Features

- **AsyncLocal-based CorrelationContext** (thread-safe, async-safe)
- **Canonical identifiers** with `tunnel*` prefix (unified naming)
- **Automatic Activity.Current sync** (OpenTelemetry integration)
- **Serilog enricher** for contextual logs (zero boilerplate)
- **Zero overhead** (AsyncLocal, no DI scope tracking)

## Context Fields

| Field | Scope | Description |
|-------|-------|-------------|
| `tunnelRequestId` | HTTP request | Request ID (from X-Tunnel-Request-Id header) |
| `tunnelClientId` | License/HW | Client identifier (HardwareThumbprint) |
| `tunnelId` | Tunnel | Persistent tunnel identity (Phase 7) |
| `tunnelProxyId` | ProxyEntry | ProxyEntry instance ID (hostname/pod) |
| `tunnelSessionId` | TCP session | Optional TCP session ID |

## Installation

### From GitHub Packages

```bash
dotnet add package Tunnel.Telemetry --source "https://nuget.pkg.github.com/GeorgyBatalov/index.json"
```

### From source

```bash
dotnet add reference path/to/tunnel2-common/src/Tunnel.Telemetry/Tunnel.Telemetry.csproj
```

## Usage

### 1. Register in DI

```csharp
// Program.cs
services.AddSingleton<ICorrelationContextAccessor, CorrelationContextAccessor>();
```

### 2. Configure Serilog

```csharp
builder.Host.UseSerilog((context, services, config) =>
{
    var accessor = services.GetRequiredService<ICorrelationContextAccessor>();

    config
        .Enrich.FromLogContext()
        .Enrich.With(new TunnelCorrelationEnricher(accessor))
        .WriteTo.Console(new JsonFormatter())
        .WriteTo.OpenSearch(...);
});
```

### 3. Set context in your code

```csharp
public class MyService
{
    private readonly ICorrelationContextAccessor _accessor;

    public MyService(ICorrelationContextAccessor accessor)
    {
        _accessor = accessor;
    }

    public async Task ProcessRequest(string tunnelId, string clientId)
    {
        // ONE line - sets both context and Activity tags!
        _accessor.Current.SetTunnelContext(
            tunnelId: tunnelId,
            clientId: clientId
        );

        // All logs from now on will have tunnelId and tunnelClientId!
        _logger.LogInformation("Processing request");
        // → {
        //   "tunnelId": "6f43bd87-...",
        //   "tunnelClientId": "hwid-5ac7..."
        // }
    }
}
```

### 4. Individual field setters

```csharp
var ctx = _accessor.Current;

// Each setter updates BOTH context and Activity.Current
ctx.SetTunnelRequestId("req-123");
ctx.SetTunnelClientId("client-456");
ctx.SetTunnelId("tunnel-789");
ctx.SetTunnelProxyId("proxy-1");
ctx.SetTunnelSessionId("sess-abc");

// Custom tags (low cardinality only!)
ctx.AddCustomTag("tier", "professional");
```

## Benefits

**Before:**
```csharp
_logger.LogInformation("Processing request for tunnel {TunnelId}, client {ClientId}", tunnelId, clientId);
// Boilerplate in every log call!
```

**After:**
```csharp
_accessor.Current.SetTunnelContext(tunnelId, clientId);
_logger.LogInformation("Processing request");
// Context added automatically! ✨
```

**Result in OpenSearch:**
```json
{
  "@timestamp": "2025-11-27T10:30:45Z",
  "level": "Information",
  "message": "Processing request",
  "tunnelId": "6f43bd87-...",
  "tunnelClientId": "hwid-5ac7..."
}
```

## AsyncLocal Behavior

- **Thread-safe:** Each async flow has its own context
- **Async/await safe:** Context flows through Task continuations
- **Task.Run flows:** Context flows by design (desired behavior)
- **Zero overhead:** No locks, no allocations

## OpenTelemetry Integration

All `Set*()` methods automatically sync with `Activity.Current`:

```csharp
ctx.SetTunnelId("tunnel-123");
// ↓ Updates both:
// 1. ctx.TunnelId = "tunnel-123"
// 2. Activity.Current?.SetTag("tunnelId", "tunnel-123")
```

View in Jaeger UI:
- Filter by `tunnelId:"tunnel-123"`
- See complete span hierarchy
- Click from logs → traces

## Development

### Building

```bash
dotnet build Tunnel.Common.sln
```

### Testing

Run all unit tests:

```bash
dotnet test Tunnel.Common.sln
```

All 17 tests passing ✅

### Publishing

Packages are automatically published to GitHub Packages when a new release is created. See `.github/workflows/nuget.yml`.

## Architecture

```
CorrelationContext (data model)
    ↓
CorrelationContextAccessor (AsyncLocal storage)
    ↓
Extensions (Set* methods)
    ↓
    ├──→ Updates CorrelationContext
    └──→ Updates Activity.Current (OpenTelemetry)
         ↓
         ├──→ Serilog enricher reads context
         └──→ OTLP exporter sends traces
```

## Version

**v0.1.0** - Initial release (Phase 8, Week 1, Day 1-2)

## License

MIT

## Repository Structure

```
tunnel2-common/
├── .github/
│   └── workflows/
│       ├── build-and-test.yml    # CI - Build & Test on every commit
│       └── nuget.yml              # CD - Publish packages on release
├── src/
│   ├── Tunnel.Telemetry/          # Distributed tracing library
│   └── Tunnel.Telemetry.UnitTests/
└── Tunnel.Common.sln
```

## Links

- [Phase 8 Design Document](../PHASE8_DISTRIBUTED_TRACING_DESIGN_V2.md)
- [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/)
- [Serilog](https://serilog.net/)
- [GitHub Repository](https://github.com/GeorgyBatalov/tunnel2-common)
