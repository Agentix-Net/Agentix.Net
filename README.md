# Agentix.Net

A modular .NET framework for building AI-powered applications with multiple providers and communication channels.

Build intelligent applications that can seamlessly switch between AI providers (OpenAI, Claude, Azure OpenAI) and communicate through various channels (Console, Slack, Teams, Web APIs) with minimal code changes.

## 🚀 Quick Start

### Installation

Install the core package and your desired providers/channels via NuGet:

```bash
# Core framework
dotnet add package Agentix.Core

# AI Providers (choose one or more)
dotnet add package Agentix.Providers.Claude
dotnet add package Agentix.Providers.OpenAI       # Coming soon
dotnet add package Agentix.Providers.AzureOpenAI  # Coming soon

# Channels (choose based on your application type)
dotnet add package Agentix.Channels.Console
dotnet add package Agentix.Channels.Slack         # Coming soon
dotnet add package Agentix.Channels.WebApi        # Coming soon
```

### Simple Console Application

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude;
using Agentix.Channels.Console;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// Add Agentix framework
builder.Services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = "your-claude-api-key";
        options.DefaultModel = "claude-3-sonnet-20241022";
    })
    .AddConsoleChannel();

var app = builder.Build();

// Get the orchestrator and start the application
var orchestrator = app.Services.GetRequiredService<IAgentixOrchestrator>();
await orchestrator.StartAsync();

await app.RunAsync();
```

### Web API Application

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.OpenAI;
using Agentix.Channels.WebApi;

var builder = WebApplication.CreateBuilder(args);

// Add Agentix framework
builder.Services.AddAgentixCore()
    .AddOpenAIProvider(options =>
    {
        options.ApiKey = builder.Configuration["OpenAI:ApiKey"];
        options.DefaultModel = "gpt-4";
    })
    .AddWebApiChannel();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

## 📦 Available Packages

### Core Framework
- **Agentix.Core** - Core abstractions, orchestration, and dependency injection

### AI Providers
- **Agentix.Providers.Claude** ✅ - Anthropic Claude integration
- **Agentix.Providers.OpenAI** 🚧 - OpenAI GPT models
- **Agentix.Providers.AzureOpenAI** 🚧 - Azure OpenAI Service
- **Agentix.Providers.Ollama** 🚧 - Local model hosting

### Communication Channels
- **Agentix.Channels.Console** ✅ - Console/terminal interface
- **Agentix.Channels.Slack** 🚧 - Slack bot integration
- **Agentix.Channels.Teams** 🚧 - Microsoft Teams integration
- **Agentix.Channels.WebApi** 🚧 - HTTP REST API interface

### Advanced Features
- **Agentix.Context** 🚧 - Conversation memory and state management
- **Agentix.Tools** 🚧 - Function calling and tool integration
- **Agentix.Rag** 🚧 - Retrieval Augmented Generation

## 💻 Usage Examples

### Multiple AI Providers

```csharp
services.AddAgentixCore()
    .AddClaudeProvider(options => options.ApiKey = claudeKey)
    .AddOpenAIProvider(options => options.ApiKey = openAiKey);

// The framework will automatically route requests to the best provider
// or you can specify which one to use
var response = await orchestrator.ProcessMessageAsync(message, "claude");
```

### Multi-Channel Support

```csharp
services.AddAgentixCore()
    .AddClaudeProvider(options => options.ApiKey = apiKey)
    .AddConsoleChannel()
    .AddSlackChannel(options => options.BotToken = slackToken)
    .AddWebApiChannel();

// Messages from any channel are handled uniformly
// The framework handles channel-specific formatting automatically
```

### Custom Configuration

```csharp
services.AddAgentixCore(options =>
{
    options.DefaultSystemPrompt = "You are a helpful assistant specialized in .NET development.";
    options.EnableCostTracking = true;
    options.MaxConcurrentRequests = 10;
})
.AddClaudeProvider(options =>
{
    options.ApiKey = configuration["Claude:ApiKey"];
    options.DefaultModel = "claude-3-sonnet-20241022";
    options.Temperature = 0.7f;
    options.MaxTokens = 4000;
});
```

## ⚙️ Configuration

### Using appsettings.json

```json
{
  "Agentix": {
    "DefaultSystemPrompt": "You are a helpful AI assistant.",
    "EnableCostTracking": true
  },
  "Claude": {
    "ApiKey": "your-claude-api-key",
    "DefaultModel": "claude-3-sonnet-20241022",
    "Temperature": 0.7,
    "MaxTokens": 4000
  },
  "OpenAI": {
    "ApiKey": "your-openai-api-key",
    "DefaultModel": "gpt-4",
    "Organization": "your-org-id"
  }
}
```

### Using Environment Variables

```bash
# Claude
CLAUDE_API_KEY=your-claude-api-key

# OpenAI
OPENAI_API_KEY=your-openai-api-key
OPENAI_ORGANIZATION=your-org-id

# Slack
SLACK_BOT_TOKEN=your-slack-bot-token
SLACK_SIGNING_SECRET=your-signing-secret
```

## 🎯 Use Cases

### Chatbots and Virtual Assistants
Build intelligent bots that can operate across multiple platforms (Slack, Teams, Web) while maintaining conversation context and utilizing the best AI model for each task.

### API Services
Create AI-powered REST APIs that can switch between different AI providers based on cost, performance, or availability requirements.

### Console Tools
Build command-line AI tools for developers, with rich interaction capabilities and easy integration into existing workflows.

### Multi-Modal Applications
Combine text, image, and file processing capabilities across different AI providers in a single application.

## 🔧 Advanced Features

### Provider Routing
```csharp
// Automatic routing based on request characteristics
var response = await orchestrator.ProcessMessageAsync(message);

// Manual provider selection
var response = await orchestrator.ProcessMessageAsync(message, "claude");

// Fallback handling
services.AddAgentixCore(options =>
{
    options.FallbackProvider = "openai";
    options.EnableAutomaticFailover = true;
});
```

### Cost Tracking
```csharp
var response = await orchestrator.ProcessMessageAsync(message);
Console.WriteLine($"Request cost: ${response.EstimatedCost:F4}");
Console.WriteLine($"Tokens used: {response.Usage.TotalTokens}");
```

### Health Monitoring
```csharp
var status = await orchestrator.GetStatusAsync();
Console.WriteLine($"Healthy providers: {status.RegisteredProviders}");
Console.WriteLine($"Running channels: {status.RunningChannels}");
```

## 📖 Documentation

- [API Reference](https://docs.agentix.net/api) - Complete API documentation
- [Samples](samples/) - Example applications and use cases
- [Architecture Guide](docs/agentix_design_document.md) - Detailed framework design

## 🤝 Community

- [GitHub Discussions](https://github.com/agentix/agentix.net/discussions) - Ask questions and share ideas
- [Issues](https://github.com/agentix/agentix.net/issues) - Report bugs and request features
- [Contributing Guide](CONTRIBUTING.md) - Help improve the framework

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🗺️ Roadmap

- ✅ Core framework with provider/channel abstraction
- ✅ Claude provider implementation
- ✅ Console channel implementation
- 🚧 OpenAI provider
- 🚧 Slack and Teams channels
- 🚧 Context memory system
- 🚧 Tool/function calling support
- 🚧 RAG engine integration
- 🚧 Streaming responses
- 🚧 Multi-modal support (vision, audio)

---

**Get started today!** Install `Agentix.Core` and begin building your AI-powered .NET applications.