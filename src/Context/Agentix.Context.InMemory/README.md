# Agentix.Context.InMemory

In-memory context storage implementation for Agentix.Net framework. This package provides a simple, thread-safe context store suitable for development environments and single-instance deployments.

## Features

- **Thread-safe**: Uses concurrent collections for safe multi-threaded access
- **Automatic cleanup**: Periodically removes expired conversations
- **Memory efficient**: Automatically limits message and tool result history
- **Zero dependencies**: Only requires the Core framework

## Installation

```bash
dotnet add package Agentix.Context.InMemory
```

## Usage

### Basic Setup

```csharp
using Agentix.Core.Extensions;
using Agentix.Context.InMemory.Extensions;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddAgentix()
    .AddInMemoryContext()
    .AddClaudeProvider(options => {
        options.ApiKey = "your-api-key";
    })
    .AddConsoleChannel();

var host = builder.Build();
await host.RunAsync();
```

### Advanced Configuration

```csharp
builder.Services
    .AddAgentix()
    .AddInMemoryContext(options => {
        options.CleanupInterval = TimeSpan.FromMinutes(10);
        options.MaxMessagesPerContext = 50;
        options.DefaultExpiration = TimeSpan.FromHours(2);
    });
```

## Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `CleanupInterval` | 5 minutes | How often to run expired context cleanup |
| `MaxMessagesPerContext` | 100 | Maximum messages to keep per conversation |
| `MaxToolResultsPerContext` | 50 | Maximum tool results to keep per conversation |
| `DefaultExpiration` | 4 hours | Default conversation expiration time |

## When to Use

✅ **Good for:**
- Development and testing
- Single-instance applications
- Proof of concepts
- Small-scale deployments

❌ **Not recommended for:**
- Multi-instance applications
- High-availability scenarios
- Applications requiring persistence across restarts
- Large-scale production deployments

For production environments, consider `Agentix.Context.Redis` or `Agentix.Context.Database` packages instead.

## Architecture

This package implements the `IContextStore` interface defined in `Agentix.Core`, providing:

- `InMemoryContextStore`: Main context storage implementation
- `InMemoryConversationContext`: Individual conversation state management
- Automatic expiration and cleanup mechanisms

## Thread Safety

All operations are thread-safe and can be used safely in multi-threaded environments. The implementation uses:

- `ConcurrentDictionary` for context storage
- `ConcurrentQueue` for message and tool result history
- Appropriate locking for state management 