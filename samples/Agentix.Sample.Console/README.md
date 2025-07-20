# Agentix Console Sample

This is a sample console application demonstrating the Agentix framework with Claude AI provider.

## Prerequisites

- .NET 8.0 or later
- Claude API key from Anthropic

## Getting Started

1. **Get a Claude API Key**
   - Sign up at [Anthropic Console](https://console.anthropic.com/)
   - Create an API key in your account settings

2. **Set your API key** (choose one method):
   
   **Option 1: Environment Variable**
   ```bash
   # Windows (Command Prompt)
   set CLAUDE_API_KEY=your_api_key_here
   
   # Windows (PowerShell)
   $env:CLAUDE_API_KEY="your_api_key_here"
   
   # macOS/Linux
   export CLAUDE_API_KEY=your_api_key_here
   ```
   
   **Option 2: Command Line Argument**
   ```bash
   dotnet run -- --api-key your_api_key_here
   ```

3. **Run the application**
   ```bash
   dotnet run
   ```

## Usage

Once the application starts, you'll see a welcome message and a prompt:

```
🚀 Welcome to Agentix Console!
Type your messages to chat with AI. Commands:
  /help - Show help
  /quit - Exit the application

YourUsername>
```

### Available Commands

- `/help` - Show help information
- `/quit` or `/exit` - Exit the application
- Type any message to chat with Claude AI

### Example Session

```
YourUsername> How do I create a simple HTTP client in C#?
🤖 Here's a simple way to create an HTTP client in C#:

```csharp
using var client = new HttpClient();

// GET request
string response = await client.GetStringAsync("https://api.example.com/data");

// POST request with JSON
var jsonContent = JsonSerializer.Serialize(new { name = "John", age = 30 });
var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
var postResponse = await client.PostAsync("https://api.example.com/users", content);
```

For dependency injection in ASP.NET Core:
```csharp
builder.Services.AddHttpClient();
```

   📊 Tokens: 22/143 | Cost: $0.0003 | Time: 756ms | Provider: claude

YourUsername> What's the best practice for disposing HttpClient?
🤖 Use `using var client = new HttpClient()` for short-lived clients, or register as singleton via DI (`AddHttpClient()`) for better performance and connection pooling.

   📊 Tokens: 18/45 | Cost: $0.0001 | Time: 432ms | Provider: claude

YourUsername> /quit
🤖 Goodbye! 👋
```

## Features Demonstrated

This sample shows:

- ✅ **Modular Architecture** - Core, Provider, and Channel packages working together
- ✅ **Dependency Injection** - Services automatically registered and wired up
- ✅ **AI Provider Abstraction** - Easy to swap Claude for other providers
- ✅ **Channel Abstraction** - Console channel with commands and formatting
- ✅ **Custom System Prompts** - Users can set personalized AI behavior and context
- ✅ **Cost Tracking** - Token usage and cost estimation
- ✅ **Error Handling** - Graceful error handling and user feedback
- ✅ **Logging** - Structured logging throughout the system

## System Prompt Configuration

The Agentix framework supports custom system prompts, allowing SDK developers to configure AI behavior:

### What are System Prompts?

System prompts are instructions that define the AI's role, personality, and behavior. They set the context for how the AI should respond to user messages.

### How to Configure (for SDK Developers)

System prompts are configured when setting up the Agentix services:

```csharp
// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Configure system prompts
serviceProvider.ConfigureSystemPrompts(prompts =>
{
    // Set default system prompt for all interactions
    prompts.SetDefaultSystemPrompt("You are a helpful AI assistant...");
    
    // Set channel-specific prompts
    prompts.SetChannelSystemPrompt("console", "You are concise and direct...");
    prompts.SetChannelSystemPrompt("slack", "You are friendly and use emojis...");
    
    // Set user-specific prompts
    prompts.SetUserSystemPrompt("admin", "You are a technical expert...");
});
```

### System Prompt Priority

The framework uses this priority order:
1. **User-specific prompts** (highest priority)
2. **Channel-specific prompts**
3. **Default prompt** (fallback)

### Example Configurations

```csharp
// Coding Assistant
prompts.SetDefaultSystemPrompt(@"
You are an expert software engineer specializing in .NET and C#. 
Provide practical code examples and follow best practices.
Be concise but thorough in your explanations.");

// Support Bot
prompts.SetChannelSystemPrompt("support", @"
You are a helpful customer support assistant.
Be empathetic, patient, and solution-focused.
Always ask clarifying questions when needed.");

// Executive Assistant
prompts.SetUserSystemPrompt("ceo", @"
You are an executive assistant for busy leadership.
Provide brief, actionable summaries and recommendations.
Focus on business impact and strategic insights.");
```

### Configuration Tips

- Keep prompts clear and specific
- Define the AI's expertise area and role
- Specify the desired response style (concise, detailed, etc.)
- Include formatting and tone preferences
- Consider the target audience for each channel/user

## Configuration

The sample uses these default settings:

- **Model**: `claude-3-haiku-20240307` (fastest and most cost-effective)
- **Max Tokens**: 1000
- **Temperature**: 0.7
- **Timeout**: 30 seconds

You can modify these in the `Program.cs` file if needed.

## Next Steps

To extend this sample:

1. **Add more providers** - OpenAI, Azure OpenAI, etc.
2. **Add tools** - Web search, file operations, APIs
3. **Add other channels** - Slack, Teams, Web API
4. **Add context memory** - Conversation history and state
5. **Add RAG capabilities** - Document search and retrieval

## Troubleshooting

### Common Issues

**"Claude API key not found"**
- Make sure you've set the API key using one of the methods above
- Verify the API key is valid in the Anthropic Console

**"Error generating response with Claude"**
- Check your internet connection
- Verify your API key has sufficient credits
- Check the logs for specific error details

**Application crashes on startup**
- Ensure all NuGet packages are restored: `dotnet restore`
- Make sure you're using .NET 8.0 or later: `dotnet --version` 