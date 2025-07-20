# Agentix.Channels.Console

Console and terminal interface for the Agentix framework, providing an interactive command-line AI experience.

## Overview

The Console channel adapter enables direct terminal interaction with AI agents, making it perfect for:

- **Development and Testing**: Quick AI interaction during development
- **Command-Line Tools**: Building AI-powered CLI applications
- **Local Debugging**: Testing AI responses in a controlled environment
- **Batch Processing**: Scripted AI interactions for automation

## Features

- 🖥️ **Interactive Console**: Real-time chat with AI in your terminal
- ⌨️ **Command Support**: Built-in commands for session management
- 📊 **Metadata Display**: Optional cost, token, and timing information
- 🎨 **Colored Output**: Syntax highlighting and formatted responses
- 📝 **Session History**: Conversation persistence across restarts
- 🔄 **Auto-restart**: Graceful error recovery
- ⏹️ **Clean Exit**: Proper shutdown handling

## Installation

```bash
# Install the Console channel
dotnet add package Agentix.Channels.Console

# You'll also need the core framework and a provider
dotnet add package Agentix.Core
dotnet add package Agentix.Providers.Claude
```

## Quick Start

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateDefaultBuilder(args);

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

// Build and run - the console will start automatically
await builder.BuildAndRunAgentixAsync();
```

## Configuration

### ConsoleChannelOptions

Complete configuration options:

```csharp
services.AddConsoleChannel(options =>
{
    // Welcome and branding
    options.WelcomeMessage = "Welcome to your AI assistant!";
    options.BotName = "Assistant";
    options.PromptSymbol = "> ";
    
    // Display options
    options.ShowMetadata = true;         // Show cost, tokens, timing
    options.ShowTimestamps = false;      // Show message timestamps
    options.EnableColors = true;         // Colored output
    options.ClearOnStart = false;        // Clear console on startup
    
    // Input handling
    options.MaxInputLength = 4000;       // Maximum input characters
    options.EnableMultiLine = true;      // Allow multi-line input (Ctrl+Enter)
    options.TrimInput = true;            // Remove leading/trailing whitespace
    
    // Session management
    options.EnableHistory = true;        // Save conversation history
    options.HistoryFile = "agentix_history.json"; // History file name
    options.MaxHistoryEntries = 100;     // Maximum history entries
    
    // Error handling
    options.ShowErrors = true;           // Display detailed errors
    options.AutoRestart = true;          // Restart on critical errors
    options.ExitCommands = new[] { "exit", "quit", "bye" }; // Exit commands
});
```

### Environment Variables

```bash
export AGENTIX_CONSOLE_WELCOME="Hello! I'm your AI assistant."
export AGENTIX_CONSOLE_BOT_NAME="Claude"
export AGENTIX_CONSOLE_SHOW_METADATA="true"
export AGENTIX_CONSOLE_ENABLE_COLORS="true"
```

## Usage Examples

### Basic Console Application

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Microsoft.Extensions.Hosting;

// One-liner setup and run
await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddAgentixCore(options =>
        {
            options.SystemPrompt = "You are a helpful programming assistant.";
        })
        .AddClaudeProvider(options =>
        {
            options.ApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
            options.DefaultModel = "claude-3-haiku-20240307";
        })
        .AddConsoleChannel(options =>
        {
            options.WelcomeMessage = "🤖 Programming Assistant Ready!";
            options.ShowMetadata = true;
        }))
    .BuildAndRunAgentixAsync();
```

### Advanced Configuration

```csharp
services.AddAgentixCore(options =>
{
    options.SystemPrompt = @"You are an expert software architect. 
        Provide practical, actionable advice with code examples.";
    options.EnableCostTracking = true;
})
.AddClaudeProvider(options =>
{
    options.ApiKey = configuration["Claude:ApiKey"];
    options.DefaultModel = "claude-3-sonnet-20241022";
    options.Temperature = 0.7f;
    options.MaxTokens = 2000;
})
.AddConsoleChannel(options =>
{
    options.WelcomeMessage = "🏗️ Software Architecture Assistant";
    options.BotName = "Architect";
    options.PromptSymbol = "🏗️ > ";
    options.ShowMetadata = true;
    options.EnableColors = true;
    options.EnableHistory = true;
    options.MaxInputLength = 8000; // Longer inputs for code
});
```

### Multiple Providers with Console

```csharp
services.AddAgentixCore()
    // Fast model for simple questions
    .AddClaudeProvider("fast", options =>
    {
        options.ApiKey = apiKey;
        options.DefaultModel = "claude-3-haiku-20240307";
        options.MaxTokens = 500;
    })
    // Powerful model for complex questions
    .AddClaudeProvider("smart", options =>
    {
        options.ApiKey = apiKey;
        options.DefaultModel = "claude-3-sonnet-20241022";
        options.MaxTokens = 2000;
    })
    .AddConsoleChannel(options =>
    {
        options.WelcomeMessage = "Choose your AI: Type 'fast' or 'smart' to select model";
        options.ShowMetadata = true;
    });
```

## Built-in Commands

The Console channel includes several built-in commands:

### Session Commands
- `exit`, `quit`, `bye` - Exit the application
- `clear` - Clear the console screen
- `help` - Show available commands
- `history` - Show conversation history
- `reset` - Clear conversation context

### Debug Commands (when ShowMetadata = true)
- `status` - Show system status
- `providers` - List available AI providers
- `cost` - Show session cost summary
- `tokens` - Show token usage statistics

### Example Session

```
🤖 Programming Assistant Ready!

> hello
Assistant: Hello! I'm your AI programming assistant. How can I help you with your 
coding questions today?

[Cost: $0.0023 | Tokens: 89 | Time: 1.2s | Model: claude-3-haiku-20240307]

> help
Available commands:
- exit, quit, bye: Exit the application
- clear: Clear the console screen
- help: Show this help message
- history: Show conversation history
- reset: Clear conversation context
- status: Show system status

> explain async/await in C#
Assistant: Async/await in C# is a powerful pattern for handling asynchronous 
operations without blocking the calling thread...

[Cost: $0.0156 | Tokens: 634 | Time: 2.1s | Model: claude-3-haiku-20240307]

> exit
Goodbye! Session cost: $0.0179
```

## Customization

### Custom Welcome Message

```csharp
options.WelcomeMessage = @"
╔══════════════════════════════════════╗
║        🚀 AI Code Assistant          ║
║                                      ║
║  Type 'help' for commands            ║
║  Type 'exit' to quit                 ║
╚══════════════════════════════════════╝
";
```

### Custom Commands

```csharp
services.AddConsoleChannel(options =>
{
    options.ExitCommands = new[] { "exit", "quit", "bye", "logout" };
    // Custom command handling can be added via events
});
```

### Color Themes

```csharp
services.AddConsoleChannel(options =>
{
    options.EnableColors = true;
    
    // Colors are automatically applied:
    // - User input: White
    // - Assistant responses: Cyan
    // - Metadata: Gray
    // - Errors: Red
    // - Commands: Yellow
});
```

## Integration Examples

### Development Tool

```csharp
// Perfect for a development helper tool
services.AddAgentixCore(options =>
{
    options.SystemPrompt = @"You are a .NET development assistant. Help with:
        - Code reviews and suggestions
        - Debugging assistance  
        - Architecture advice
        - Best practices";
})
.AddClaudeProvider(options =>
{
    options.ApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
    options.DefaultModel = "claude-3-sonnet-20241022";
})
.AddConsoleChannel(options =>
{
    options.WelcomeMessage = "⚡ .NET Development Assistant";
    options.BotName = "DevBot";
    options.ShowMetadata = true;
    options.EnableHistory = true;
});
```

### Research Assistant

```csharp
// Research and analysis tool
services.AddAgentixCore(options =>
{
    options.SystemPrompt = "You are a research assistant specialized in technology analysis.";
})
.AddClaudeProvider(options =>
{
    options.DefaultModel = "claude-3-opus-20240229"; // Most capable model
    options.MaxTokens = 4000; // Longer responses
})
.AddConsoleChannel(options =>
{
    options.WelcomeMessage = "📚 Research Assistant - Ask me about any technology topic";
    options.ShowMetadata = true;
    options.MaxInputLength = 10000; // Longer research queries
});
```

### Batch Processing

```csharp
// For scripted or batch operations
public class BatchProcessor
{
    public async Task ProcessQuestionsFromFile(string filePath)
    {
        var orchestrator = serviceProvider.GetRequiredService<IAgentixOrchestrator>();
        var questions = await File.ReadAllLinesAsync(filePath);
        
        foreach (var question in questions)
        {
            var message = new IncomingMessage
            {
                Content = question,
                UserId = "batch-processor",
                Channel = "console"
            };
            
            var response = await orchestrator.ProcessMessageAsync(message);
            Console.WriteLine($"Q: {question}");
            Console.WriteLine($"A: {response.Content}");
            Console.WriteLine($"Cost: ${response.EstimatedCost:F4}");
            Console.WriteLine("---");
        }
    }
}
```

## Performance Considerations

### For High-Volume Usage

```csharp
services.AddConsoleChannel(options =>
{
    options.ShowMetadata = false;    // Reduce output overhead
    options.EnableColors = false;    // Faster rendering
    options.EnableHistory = false;   // Reduce memory usage
    options.MaxInputLength = 2000;   // Limit input size
});

services.AddClaudeProvider(options =>
{
    options.DefaultModel = "claude-3-haiku-20240307"; // Fastest model
    options.MaxTokens = 500;         // Shorter responses
    options.Temperature = 0.3f;      // More consistent responses
});
```

### For Development/Testing

```csharp
services.AddConsoleChannel(options =>
{
    options.ShowMetadata = true;     // Full debugging info
    options.EnableColors = true;     // Better UX
    options.EnableHistory = true;    // Session persistence
    options.ShowTimestamps = true;   // Timing information
    options.ShowErrors = true;       // Detailed error info
});
```

## Troubleshooting

### Common Issues

**Console exits immediately**
```csharp
// Make sure to use BuildAndRunAgentixAsync() or RunAgentixAsync()
await builder.BuildAndRunAgentixAsync(); // ✅ Correct
// Not: await builder.Build().RunAsync(); // ❌ Wrong
```

**No response from AI**
```csharp
// Ensure you have a provider configured
services.AddAgentixCore()
    .AddClaudeProvider(options => { /* configure */ }) // ✅ Required
    .AddConsoleChannel();
```

**Colors not showing**
```csharp
// Enable colors and ensure console supports them
options.EnableColors = true;

// Test if your terminal supports colors:
// Windows: Use Windows Terminal or PowerShell Core
// macOS/Linux: Most terminals support colors by default
```

**History not persisting**
```csharp
options.EnableHistory = true;
options.HistoryFile = "chat_history.json"; // Ensure write permissions
```

**Input too long errors**
```csharp
options.MaxInputLength = 8000; // Increase limit for longer inputs
```

## API Reference

See the [Agentix.Core documentation](../../Core/Agentix.Core/README.md) for the `IChannelAdapter` interface that this channel implements.

## Related Packages

- [Agentix.Core](../../Core/Agentix.Core/README.md) - Core framework (required)
- [Agentix.Providers.Claude](../../Providers/Agentix.Providers.Claude/README.md) - Claude AI provider
- [Agentix.Channels.Slack](../Agentix.Channels.Slack/README.md) - Slack integration

## Examples

See the [Console Sample](../../../samples/Agentix.Sample.Console/README.md) for a complete working example.

## License

This project is licensed under the MIT License. 