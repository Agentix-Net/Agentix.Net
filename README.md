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
dotnet add package Agentix.Providers.Claude       # ✅ Available now
dotnet add package Agentix.Providers.OpenAI       # 🚧 Coming soon
dotnet add package Agentix.Providers.AzureOpenAI  # 🚧 Coming soon

# Channels (choose based on your application type)  
dotnet add package Agentix.Channels.Console       # ✅ Available now
dotnet add package Agentix.Channels.Slack         # ✅ Available now
dotnet add package Agentix.Channels.WebApi        # 🚧 Coming soon
```

### Simple Console Application

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

// Configure Agentix
builder.Services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = "your-claude-api-key";
        options.DefaultModel = "claude-3-sonnet-20241022";
    })
    .AddConsoleChannel(options =>
    {
        options.WelcomeMessage = "Welcome to your AI assistant!";
        options.ShowMetadata = true;
    });

// Build and run - that's it!
await builder.BuildAndRunAgentixAsync();
```

### Web API Application

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.OpenAI.Extensions;
using Agentix.Channels.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Configure Agentix
builder.Services.AddAgentixCore()
    .AddOpenAIProvider(options =>
    {
        options.ApiKey = builder.Configuration["OpenAI:ApiKey"];
        options.DefaultModel = "gpt-4";
    })
    .AddWebApiChannel();

builder.Services.AddControllers();

var app = builder.Build();

// Start Agentix and run the web API
await app.StartAgentixAsync();
app.MapControllers();
app.Run();
```

## 📦 Available Packages

### Core Framework
- **Agentix.Core** - Core abstractions, orchestration, and dependency injection

### AI Providers
- **Agentix.Providers.Claude** ✅ - Anthropic Claude integration ([Documentation](src/Providers/Agentix.Providers.Claude/README.md))
- **Agentix.Providers.OpenAI** 🚧 - OpenAI GPT models
- **Agentix.Providers.AzureOpenAI** 🚧 - Azure OpenAI Service
- **Agentix.Providers.Ollama** 🚧 - Local model hosting

### Communication Channels
- **Agentix.Channels.Console** ✅ - Console/terminal interface ([Documentation](src/Channels/Agentix.Channels.Console/README.md))
- **Agentix.Channels.Slack** ✅ - Slack bot integration ([Documentation](src/Channels/Agentix.Channels.Slack/README.md))
- **Agentix.Channels.Teams** 🚧 - Microsoft Teams integration
- **Agentix.Channels.WebApi** 🚧 - HTTP REST API interface

### Advanced Features
- **Agentix.Context** 🚧 - Conversation memory and state management
- **Agentix.Tools** 🚧 - Function calling and tool integration
- **Agentix.Rag** 🚧 - Retrieval Augmented Generation

## 💻 Usage Examples

### Multiple AI Providers

```csharp
var builder = Host.CreateDefaultBuilder(args);

builder.Services.AddAgentixCore()
    .AddClaudeProvider(options => options.ApiKey = claudeKey)
    .AddOpenAIProvider(options => options.ApiKey = openAiKey)
    .AddConsoleChannel();

// Build and run - the framework handles provider routing automatically
await builder.BuildAndRunAgentixAsync();
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
    options.SystemPrompt = "You are a helpful assistant specialized in .NET development.";
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

## 🚀 Starting Your Application

Agentix provides clean, simple APIs for starting your applications without manual service resolution:

### Option 1: One-liner (Recommended)
```csharp
// Configure and run in one line
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddAgentixCore()
        .AddClaudeProvider(options => options.ApiKey = apiKey)
        .AddConsoleChannel())
    .BuildAndRunAgentixAsync();
```

### Option 2: Traditional approach  
```csharp
// Build first, then run
var app = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddAgentixCore()
        .AddClaudeProvider(options => options.ApiKey = apiKey)
        .AddConsoleChannel())
    .Build();

await app.RunAgentixAsync();
```

### Option 3: For Web APIs
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAgentixCore()
    .AddOpenAIProvider(options => options.ApiKey = apiKey)
    .AddWebApiChannel();

var app = builder.Build();
await app.StartAgentixAsync(); // Start Agentix only
app.MapControllers();
app.Run(); // Run the web server
```

## ⚙️ Configuration

### Using appsettings.json

```json
{
  "Agentix": {
    "SystemPrompt": "You are a helpful AI assistant.",
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
Build command-line AI tools for developers with minimal code. Just configure your providers and channels, then call `BuildAndRunAgentixAsync()` - the console channel handles all user interaction automatically, including conversation loops, command processing, and error handling.

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

- **[Core Framework](src/Core/Agentix.Core/README.md)** - Core abstractions and interfaces
- **[Claude Provider](src/Providers/Agentix.Providers.Claude/README.md)** - Anthropic Claude integration
- **[Console Channel](src/Channels/Agentix.Channels.Console/README.md)** - Terminal interface
- **[Slack Channel](src/Channels/Agentix.Channels.Slack/README.md)** - Slack bot integration
- **[Samples](samples/)** - Example applications and use cases
- **[Architecture Guide](docs/agentix_design_document.md)** - Detailed framework design

## 🤝 Community

- [GitHub Discussions](https://github.com/agentix/agentix.net/discussions) - Ask questions and share ideas
- [Issues](https://github.com/agentix/agentix.net/issues) - Report bugs and request features
- [Contributing Guide](CONTRIBUTING.md) - Help improve the framework

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🗺️ Roadmap

- ✅ Core framework with provider/channel abstraction
f- ✅ Claude provider implementation  
- ✅ Console channel implementation
- ✅ Slack channel implementation
- 🚧 OpenAI provider
- 🚧 Teams channel
- 🚧 Context memory system
- 🚧 Tool/function calling support
- 🚧 RAG engine integration
- 🚧 Streaming responses
- 🚧 Multi-modal support (vision, audio)

---

**Get started today!** Install `Agentix.Core` and begin building your AI-powered .NET applications.