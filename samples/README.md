# Agentix Samples

This directory contains sample applications demonstrating how to use the Agentix framework with different channels and providers.

## Existing Samples

- **Agentix.Sample.Console** - Command-line application demonstrating console channel with Claude provider

## Running Samples

### Console Sample

The console sample shows the basic Agentix pattern:

```csharp
var builder = Host.CreateApplicationBuilder(args);

// Add Agentix framework
builder.Services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = claudeApiKey;
        options.DefaultModel = "claude-3-haiku-20240307";
        options.Temperature = 0.7f;
        options.MaxTokens = 1000;
    })
    .AddConsoleChannel();

var app = builder.Build();

// Get the orchestrator and start the application
var orchestrator = app.Services.GetRequiredService<IAgentixOrchestrator>();
await orchestrator.StartAsync();
```

**To run:**
```bash
cd samples/Agentix.Sample.Console

# Option 1: Environment variable
set CLAUDE_API_KEY=your-claude-api-key
dotnet run

# Option 2: Command line argument
dotnet run -- --api-key your-claude-api-key
```

## Creating New Samples

To create a new sample application:

1. Create a new project directory: `samples/Agentix.Sample.{SampleName}/`
2. Add project references to the Agentix components you want to demonstrate
3. Follow the established patterns from existing samples
4. Add your project to the solution file
5. Document setup and usage in the project README

### Example Sample Structure

```
samples/Agentix.Sample.Web/
├── Agentix.Sample.Web.csproj
├── Program.cs
├── Controllers/
│   └── ChatController.cs
├── Views/
│   └── Chat/
└── README.md
```

### Basic Sample Pattern

All samples follow this pattern:

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.{ProviderName}.Extensions;
using Agentix.Channels.{ChannelName}.Extensions;

var builder = Host.CreateApplicationBuilder(args); // or WebApplication.CreateBuilder(args)

// Add Agentix framework
builder.Services.AddAgentixCore()
    .Add{Provider}Provider(options => { /* configure */ })
    .Add{Channel}Channel(options => { /* configure */ });

var app = builder.Build();

// Start the application
var orchestrator = app.Services.GetRequiredService<IAgentixOrchestrator>();
await orchestrator.StartAsync();

await app.RunAsync();
```

### Sample Naming Convention

- `Agentix.Sample.Console` - Console applications
- `Agentix.Sample.Web` - Web applications  
- `Agentix.Sample.Slack` - Slack bot examples
- `Agentix.Sample.Teams` - Teams bot examples
- `Agentix.Sample.MultiProvider` - Multiple provider examples

## Available Samples

### [Console Sample](Agentix.Sample.Console/)
**Basic AI chat in console**
- Simple setup with Claude provider
- Console channel integration
- Configuration examples

### [Slack Bot](Agentix.Sample.Slack/)
**Deploy your AI to Slack**
- Full Slack integration with events and commands
- Multi-channel support
- Production-ready bot setup

### [GitHub Repository Search](Agentix.Sample.RAG.Console/)
**AI that understands your codebase**
- Search multiple GitHub repositories
- Natural language code queries
- Code examples with GitHub links
- Conversation context with search results

## Planned Samples

- **Agentix.Sample.Web** - ASP.NET Core web application with WebAPI channel
- **Agentix.Sample.Teams** - Microsoft Teams bot
- **Agentix.Sample.MultiProvider** - Demonstrating multiple AI providers with automatic routing
- **Agentix.Sample.CustomChannel** - Custom channel implementation example 