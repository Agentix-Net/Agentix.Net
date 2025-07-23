# Agentix.Net

**Build AI applications in .NET with 3 lines of code.** Switch AI providers, deploy to multiple channels (Console, Slack), and get conversation memory - all with the same simple API.

## 🚀 Quick Start (30 seconds)

```bash
dotnet new console -n MyAIApp && cd MyAIApp
dotnet add package Agentix.Core
dotnet add package Agentix.Providers.Claude  
dotnet add package Agentix.Channels.Console
```

Replace `Program.cs`:

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddAgentixCore()
        .AddClaudeProvider(options => options.ApiKey = "your-claude-api-key")
        .AddConsoleChannel())
    .BuildAndRunAgentixAsync();
```

```bash
dotnet run
# 🎉 You now have a working AI console app!
```

## 📦 Available Packages

**Core:**
- `Agentix.Core` - Framework core

**AI Providers:**
- `Agentix.Providers.Claude` ✅ 
- `Agentix.Providers.OpenAI` 🚧 Coming soon
- `Agentix.Providers.AzureOpenAI` 🚧 Coming soon

**Channels:**
- `Agentix.Channels.Console` ✅
- `Agentix.Channels.Slack` ✅
- `Agentix.Channels.WebApi` 🚧 Coming soon

**Optional Features:**
- `Agentix.Context.InMemory` ✅ - Conversation memory

**RAG & Knowledge:**
- `Agentix.Rag.GitHub` ✅ - GitHub repository search (includes search tools)
- `Agentix.Rag.InMemory` ✅ - In-memory vector store
- `Agentix.Rag.Embeddings.OpenAI` ✅ - OpenAI text embeddings

## 🔥 Add Features

### Slack Bot
```csharp
services.AddSlackChannel(options =>
{
    options.BotToken = "xoxb-your-bot-token";
    options.SigningSecret = "your-signing-secret";
});
```

### Conversation Memory
```csharp
// Add memory so conversations remember context
services.AddInMemoryContext();

// User: "What's 2+2?"
// AI:   "2+2 equals 4."
// User: "What about the previous answer times 3?"  
// AI:   "The previous answer (4) times 3 equals 12."
```

### GitHub Repository Search (RAG)
```csharp
// Add AI-powered repository search
services.AddGitHubRAG(options => {
    options.AccessToken = "ghp_your-github-token";
    options.Repositories = [
        "https://github.com/myorg/backend-api",
        "https://github.com/myorg/frontend-app"
    ];
})
.AddOpenAIEmbeddings()      // Embedding provider (auto-detects from environment)
.AddInMemoryVectorStore();  // Vector store implementation
// GitHub search tool automatically included

// Now your AI can search your codebase!
// User: "How do we handle authentication?"
// AI: [Searches repositories and provides code examples]
```

### Multiple Channels
```csharp
// Same AI logic works across console and Slack
services.AddAgentixCore()
    .AddClaudeProvider(options => options.ApiKey = "your-claude-api-key")
    .AddConsoleChannel()   // For local testing
    .AddSlackChannel(options => {
        options.BotToken = "xoxb-your-bot-token";
        options.SigningSecret = "your-signing-secret";
    });
```

## ⚙️ Configuration

### Environment Variables
```bash
CLAUDE_API_KEY=your-claude-api-key
SLACK_BOT_TOKEN=xoxb-your-token
SLACK_SIGNING_SECRET=your-secret
```

### appsettings.json
```json
{
  "Claude": {
    "ApiKey": "your-claude-api-key",
    "DefaultModel": "claude-3-sonnet-20241022"
  },
  "Agentix": {
    "SystemPrompt": "You are a helpful assistant."
  }
}
```

### Code Configuration
```csharp
services.AddAgentixCore(options =>
{
    options.SystemPrompt = "You are a .NET expert assistant.";
    options.EnableCostTracking = true;
})
.AddClaudeProvider(options =>
{
    options.DefaultModel = "claude-3-sonnet-20241022";
    options.Temperature = 0.7f;
    options.MaxTokens = 4000;
});
```

## 🎯 Real Examples

**Console AI Assistant**
```csharp
// 3 lines = working AI assistant
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(s => s.AddAgentixCore().AddClaudeProvider(...).AddConsoleChannel())
    .BuildAndRunAgentixAsync();
```

**Slack Bot**
```csharp
// Add Slack to your console app
services.AddSlackChannel(options => {
    options.BotToken = Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN");
    options.SigningSecret = Environment.GetEnvironmentVariable("SLACK_SIGNING_SECRET");
});
```

**Multi-Channel Service**
```csharp
// Same AI logic works across console and Slack
services.AddAgentixCore()
    .AddClaudeProvider(options => options.ApiKey = "your-claude-api-key")
    .AddConsoleChannel()   // For local testing
    .AddSlackChannel(options => {
        options.BotToken = "xoxb-your-bot-token";
        options.SigningSecret = "your-signing-secret";
    });
```

## 🔗 Learn More

- **[Core Documentation](src/Core/Agentix.Core/README.md)** - Framework internals
- **[Claude Provider](src/Providers/Agentix.Providers.Claude/README.md)** - Claude integration
- **[Slack Channel](src/Channels/Agentix.Channels.Slack/README.md)** - Slack bot setup
- **[Context System](src/Context/Agentix.Context.InMemory/README.md)** - Memory management
- **[Samples](samples/)** - Complete example applications
- **[Architecture Guide](docs/agentix_design_document.md)** - Design decisions

## 🚀 Why Agentix?

✅ **3-line setup** - From zero to AI app instantly  
✅ **Multi-channel** - Same code works in console and Slack  
✅ **Claude integration** - Production-ready Anthropic Claude support  
✅ **Extensible** - Add conversation memory, new providers, custom channels  
✅ **Production ready** - Logging, error handling, rate limiting included  

## 📄 License

MIT License - build awesome AI apps! 🚀