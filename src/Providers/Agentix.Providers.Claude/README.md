# Agentix.Providers.Claude

Anthropic Claude AI integration for the Agentix framework, providing access to Claude's advanced language models.

## Overview

This package integrates Anthropic's Claude AI models with the Agentix framework, offering:

- **Multiple Models**: Support for Claude 3 Haiku, Sonnet, and Opus
- **Cost Optimization**: Automatic cost estimation and tracking
- **Streaming Support**: Real-time response streaming (coming soon)
- **Error Handling**: Robust error handling with automatic retries
- **Rate Limiting**: Built-in rate limiting to respect API limits

## Installation

```bash
# Install the Claude provider
dotnet add package Agentix.Providers.Claude

# You'll also need the core framework
dotnet add package Agentix.Core
```

## Prerequisites

- **Claude API Key**: Get your API key from [Anthropic Console](https://console.anthropic.com/)
- **Credits**: Ensure your account has sufficient credits

## Quick Start

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

builder.Services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = "your-claude-api-key-here";
        options.DefaultModel = "claude-3-sonnet-20241022";
        options.Temperature = 0.7f;
        options.MaxTokens = 1000;
    });

// Add a channel (console, slack, etc.)
// .AddConsoleChannel();

var app = builder.Build();
await app.RunAsync();
```

## Configuration

### ClaudeOptions

Complete configuration options for the Claude provider:

```csharp
services.AddClaudeProvider(options =>
{
    // Required: Your Claude API key
    options.ApiKey = "your-api-key-here";
    
    // Model Selection
    options.DefaultModel = "claude-3-sonnet-20241022"; // Default model
    
    // Generation Parameters
    options.Temperature = 0.7f;        // Creativity (0.0 - 1.0)
    options.MaxTokens = 1000;          // Maximum response length
    options.TopP = 0.9f;               // Nucleus sampling
    options.TopK = 40;                 // Top-K sampling
    
    // API Settings
    options.TimeoutSeconds = 30;       // Request timeout
    options.MaxRetries = 3;            // Retry attempts
    options.BaseUrl = "https://api.anthropic.com"; // API endpoint
    
    // Cost Management
    options.EnableCostTracking = true; // Track usage costs
    options.MaxCostPerRequest = 1.00m; // Fail if cost exceeds limit
});
```

### Environment Variables

You can also configure using environment variables:

```bash
export CLAUDE_API_KEY="your-api-key-here"
export CLAUDE_MODEL="claude-3-sonnet-20241022"
export CLAUDE_TEMPERATURE="0.7"
export CLAUDE_MAX_TOKENS="1000"
```

Then use in code:

```csharp
services.AddClaudeProvider(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
    options.DefaultModel = Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? "claude-3-haiku-20240307";
    
    if (float.TryParse(Environment.GetEnvironmentVariable("CLAUDE_TEMPERATURE"), out var temp))
        options.Temperature = temp;
        
    if (int.TryParse(Environment.GetEnvironmentVariable("CLAUDE_MAX_TOKENS"), out var maxTokens))
        options.MaxTokens = maxTokens;
});
```

### appsettings.json Configuration

```json
{
  "Claude": {
    "ApiKey": "your-api-key-here",
    "DefaultModel": "claude-3-sonnet-20241022",
    "Temperature": 0.7,
    "MaxTokens": 1000,
    "TimeoutSeconds": 30,
    "MaxRetries": 3,
    "EnableCostTracking": true
  }
}
```

Then bind the configuration:

```csharp
services.AddClaudeProvider(options =>
{
    configuration.GetSection("Claude").Bind(options);
});
```

## Available Models

### Claude 3 Models

| Model | Code | Best For | Cost (per 1K tokens) |
|-------|------|----------|---------------------|
| **Claude 3 Haiku** | `claude-3-haiku-20240307` | Fast, lightweight tasks | Input: $0.25, Output: $1.25 |
| **Claude 3 Sonnet** | `claude-3-sonnet-20241022` | Balanced performance | Input: $3.00, Output: $15.00 |
| **Claude 3 Opus** | `claude-3-opus-20240229` | Complex reasoning | Input: $15.00, Output: $75.00 |

Choose based on your needs:
- **Haiku**: Simple Q&A, basic tasks, high volume
- **Sonnet**: Most applications, good balance of cost/performance  
- **Opus**: Complex analysis, reasoning, critical applications

## Usage Examples

### Basic Text Generation

```csharp
var orchestrator = serviceProvider.GetRequiredService<IAgentixOrchestrator>();

var message = new IncomingMessage
{
    Content = "Explain quantum computing in simple terms",
    UserId = "user123",
    Channel = "console"
};

var response = await orchestrator.ProcessMessageAsync(message);
Console.WriteLine(response.Content);
Console.WriteLine($"Cost: ${response.EstimatedCost:F4}");
Console.WriteLine($"Model: {response.ModelUsed}");
```

### Multiple Models

```csharp
// Register multiple Claude models
services.AddClaudeProvider("claude-haiku", options =>
{
    options.ApiKey = apiKey;
    options.DefaultModel = "claude-3-haiku-20240307";
    options.MaxTokens = 500; // Shorter responses for speed
});

services.AddClaudeProvider("claude-sonnet", options =>
{
    options.ApiKey = apiKey;
    options.DefaultModel = "claude-3-sonnet-20241022";
    options.MaxTokens = 2000; // Longer responses for quality
});

// The framework will automatically choose the best model
// or you can specify: ProcessMessageAsync(message, providerId: "claude-sonnet")
```

### Custom System Prompts

```csharp
services.AddAgentixCore(options =>
{
    options.SystemPrompt = @"You are an expert software architect with deep knowledge of .NET, 
        microservices, and cloud architecture. Provide detailed, practical advice with 
        code examples when helpful.";
})
.AddClaudeProvider(options =>
{
    options.ApiKey = apiKey;
    options.DefaultModel = "claude-3-sonnet-20241022";
});
```

### Cost Management

```csharp
services.AddClaudeProvider(options =>
{
    options.ApiKey = apiKey;
    options.EnableCostTracking = true;
    options.MaxCostPerRequest = 0.50m; // Fail if request would cost more than $0.50
});

// Access cost information
var response = await orchestrator.ProcessMessageAsync(message);
Console.WriteLine($"Input tokens: {response.Usage.InputTokens}");
Console.WriteLine($"Output tokens: {response.Usage.OutputTokens}");
Console.WriteLine($"Total cost: ${response.EstimatedCost:F4}");
```

## Error Handling

The Claude provider includes comprehensive error handling:

```csharp
try 
{
    var response = await orchestrator.ProcessMessageAsync(message);
}
catch (ClaudeApiException ex)
{
    Console.WriteLine($"Claude API error: {ex.Message}");
    Console.WriteLine($"Error code: {ex.ErrorCode}");
    Console.WriteLine($"Rate limited: {ex.IsRateLimit}");
}
catch (InsufficientCreditsException ex)
{
    Console.WriteLine("Insufficient Claude credits");
}
catch (ModelNotAvailableException ex)
{
    Console.WriteLine($"Model {ex.ModelName} not available");
}
```

Common error scenarios:
- **Rate Limiting**: Automatic retry with exponential backoff
- **Invalid API Key**: Clear error message
- **Insufficient Credits**: Detailed credit information
- **Model Unavailable**: Fallback to alternative model (if configured)

## Best Practices

### Model Selection

```csharp
// Use Haiku for simple, high-volume tasks
services.AddClaudeProvider("fast", options =>
{
    options.DefaultModel = "claude-3-haiku-20240307";
    options.MaxTokens = 500;
    options.Temperature = 0.3f; // More focused responses
});

// Use Sonnet for general purpose
services.AddClaudeProvider("balanced", options =>
{
    options.DefaultModel = "claude-3-sonnet-20241022";
    options.MaxTokens = 1500;
    options.Temperature = 0.7f;
});

// Use Opus sparingly for complex reasoning
services.AddClaudeProvider("premium", options =>
{
    options.DefaultModel = "claude-3-opus-20240229";
    options.MaxTokens = 3000;
    options.Temperature = 0.8f;
    options.MaxCostPerRequest = 2.00m; // Higher cost limit
});
```

### Temperature Guidelines

- **0.0 - 0.3**: Factual, consistent responses (documentation, Q&A)
- **0.4 - 0.7**: Balanced creativity and consistency (most applications)
- **0.8 - 1.0**: Creative, varied responses (creative writing, brainstorming)

### Token Management

```csharp
services.AddClaudeProvider(options =>
{
    // Adjust max tokens based on use case
    options.MaxTokens = 1000;  // Good default for most responses
    
    // For longer content:
    // options.MaxTokens = 4000;  // Articles, analysis
    
    // For quick responses:
    // options.MaxTokens = 200;   // Short answers, confirmations
});
```

## Integration Examples

### With Console Channel

```csharp
services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        options.DefaultModel = "claude-3-sonnet-20241022";
    })
    .AddConsoleChannel(options =>
    {
        options.WelcomeMessage = "Welcome to your Claude-powered assistant!";
        options.ShowMetadata = true; // Show cost and token usage
    });
```

### With Slack Channel

```csharp
services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        options.DefaultModel = "claude-3-haiku-20240307"; // Faster for chat
        options.MaxTokens = 500; // Shorter responses for Slack
    })
    .AddSlackChannel(options =>
    {
        options.BotToken = Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN");
        options.RespondToMentionsOnly = true;
    });
```

## Health Checks

The provider includes health check capabilities:

```csharp
// Check if Claude API is accessible
var provider = serviceProvider.GetRequiredService<IAIProvider>();
var isHealthy = await provider.HealthCheckAsync();

if (!isHealthy)
{
    Console.WriteLine("Claude provider is not healthy");
}
```

## Troubleshooting

### Common Issues

**Invalid API Key**
```
Solution: Verify your API key at https://console.anthropic.com/
```

**Rate Limiting**
```
The provider automatically handles rate limits with exponential backoff.
Consider using multiple API keys or reducing request frequency.
```

**High Costs**
```csharp
// Set cost limits
options.MaxCostPerRequest = 0.25m;
options.EnableCostTracking = true;

// Use cheaper models for simple tasks
options.DefaultModel = "claude-3-haiku-20240307";
```

**Timeout Issues**
```csharp
// Increase timeout for complex requests
options.TimeoutSeconds = 60;
```

## API Reference

See the [Agentix.Core documentation](../../Core/Agentix.Core/README.md) for the `IAIProvider` interface that this provider implements.

## Related Packages

- [Agentix.Core](../../Core/Agentix.Core/README.md) - Core framework (required)
- [Agentix.Channels.Console](../../Channels/Agentix.Channels.Console/README.md) - Console interface
- [Agentix.Channels.Slack](../../Channels/Agentix.Channels.Slack/README.md) - Slack integration

## License

This project is licensed under the MIT License. 