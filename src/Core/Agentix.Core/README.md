# Agentix.Core

The core foundation of the Agentix framework, providing abstractions, interfaces, and orchestration logic for building AI-powered .NET applications.

## Overview

Agentix.Core is the foundational package that all other Agentix components depend on. It provides:

- **Core Abstractions**: Interfaces for AI providers and communication channels
- **Orchestration Engine**: Coordinates between providers and channels
- **Dependency Injection**: Native .NET DI integration with fluent configuration
- **Error Handling**: Comprehensive error handling and logging
- **Lifecycle Management**: Background services and cleanup

## Installation

```bash
dotnet add package Agentix.Core
```

## Quick Start

The core package by itself doesn't provide AI capabilities - you need to add at least one provider and one channel:

```csharp
using Agentix.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

// Add core framework
builder.Services.AddAgentixCore(options =>
{
    options.SystemPrompt = "You are a helpful AI assistant.";
    options.EnableCostTracking = true;
});

// Add providers and channels (examples - install separate packages)
// .AddClaudeProvider(options => options.ApiKey = "your-key")
// .AddConsoleChannel();

var app = builder.Build();
await app.RunAsync();
```

## Core Interfaces

### IAIProvider

Interface that all AI providers must implement:

```csharp
public interface IAIProvider
{
    string Name { get; }
    AICapabilities Capabilities { get; }
    
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);
    Task<decimal> EstimateCostAsync(AIRequest request);
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
}
```

### IChannelAdapter

Interface that all communication channels must implement:

```csharp
public interface IChannelAdapter
{
    string Name { get; }
    string ChannelType { get; }
    bool IsRunning { get; }
    
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    bool SupportsRichContent { get; }
    bool SupportsFileUploads { get; }
    bool SupportsInteractiveElements { get; }
}
```

### IAgentixOrchestrator

The main orchestration service that coordinates between channels and providers:

```csharp
public interface IAgentixOrchestrator
{
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    Task<AIResponse> ProcessMessageAsync(IncomingMessage message, CancellationToken cancellationToken = default);
}
```

## Configuration Options

### AgentixOptions

Core framework configuration:

```csharp
services.AddAgentixCore(options =>
{
    // System prompt used by all providers
    options.SystemPrompt = "You are a helpful AI assistant.";
    
    // Enable cost tracking across requests
    options.EnableCostTracking = true;
    
    // Maximum concurrent requests
    options.MaxConcurrentRequests = 10;
    
    // Default request timeout
    options.DefaultTimeout = TimeSpan.FromMinutes(2);
    
    // Error handling
    options.RetryAttempts = 3;
    options.RetryDelay = TimeSpan.FromSeconds(1);
});
```

## Usage Patterns

### Basic Setup

```csharp
var builder = Host.CreateDefaultBuilder(args);

builder.Services
    .AddAgentixCore()
    .AddYourProvider(options => { /* configure */ })
    .AddYourChannel(options => { /* configure */ });

var app = builder.Build();
```

### Web Application Setup

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAgentixCore()
    .AddYourProvider(options => { /* configure */ })
    .AddWebApiChannel();

var app = builder.Build();

// Start Agentix services
await app.StartAgentixAsync();

app.MapControllers();
app.Run();
```

### Manual Orchestrator Usage

```csharp
// Get the orchestrator service
var orchestrator = serviceProvider.GetRequiredService<IAgentixOrchestrator>();

// Create a message
var message = new IncomingMessage
{
    Content = "Hello, AI!",
    UserId = "user123",
    Channel = "console"
};

// Process the message
var response = await orchestrator.ProcessMessageAsync(message);

Console.WriteLine(response.Content);
Console.WriteLine($"Cost: ${response.EstimatedCost:F4}");
```

## Extension Methods

The core package provides convenient extension methods for common scenarios:

### Host Extensions

```csharp
// Build and run in one line
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services.AddAgentixCore()...)
    .BuildAndRunAgentixAsync();

// Or with more control
var app = builder.Build();
await app.RunAgentixAsync();
```

### Service Collection Extensions

```csharp
// Fluent configuration
services.AddAgentixCore()
    .AddProvider<MyProvider>()
    .AddChannel<MyChannel>();
```

## Error Handling

The core framework provides comprehensive error handling:

```csharp
services.AddAgentixCore(options =>
{
    options.RetryAttempts = 3;
    options.RetryDelay = TimeSpan.FromSeconds(2);
    options.EnableDetailedErrors = true; // For development
});
```

Errors are automatically logged and can be handled at the channel level.

## Logging

Agentix integrates with .NET's logging infrastructure:

```csharp
services.AddLogging(builder =>
{
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Information);
});

// Agentix components will automatically use the configured logger
```

## Available Packages

Once you have the core installed, add providers and channels:

### AI Providers
- `Agentix.Providers.Claude` - Anthropic Claude integration

### Communication Channels  
- `Agentix.Channels.Console` - Console/terminal interface
- `Agentix.Channels.Slack` - Slack bot integration

### Advanced Features (Coming Soon)
- `Agentix.Context` - Conversation memory and state management
- `Agentix.Tools` - Function calling and tool integration
- `Agentix.Rag` - Retrieval Augmented Generation

## Next Steps

1. **Install a Provider**: Add `Agentix.Providers.Claude` or create your own
2. **Install a Channel**: Add `Agentix.Channels.Console` or create your own  
3. **Check Examples**: See the `samples/` directory for complete examples
4. **Read Documentation**: Review the [design document](../../../docs/agentix_design_document.md)

## Contributing

See [CONTRIBUTING.md](../../../CONTRIBUTING.md) for guidelines on extending the core framework.

## License

This project is licensed under the MIT License. 