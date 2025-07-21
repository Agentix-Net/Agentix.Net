# Agentix Console Sample

This sample demonstrates how to create a simple console application using the Agentix framework with Claude AI integration.

## Features

- **Console Interface**: Direct command-line interaction with Claude AI
- **System Prompt Customization**: Configure the AI as an ADR (Architecture Decision Record) specialist
- **Cost Tracking**: Real-time token usage and cost estimation
- **Error Handling**: Robust error handling and user feedback

## Getting Started

### Prerequisites

- .NET 8.0 or later
- Claude API key from Anthropic

### Setup

1. **Get your Claude API key**:
   - Sign up at [Anthropic Console](https://console.anthropic.com/)
   - Create a new API key

2. **Run the application**:
   
   **Option 1: Command line argument**
   ```bash
   cd samples/Agentix.Sample.Console
   dotnet run -- --api-key your-claude-api-key-here
   ```
   
   **Option 2: Environment variable**
   ```bash
   set CLAUDE_API_KEY=your-claude-api-key-here
   dotnet run
   ```
   
   **Option 3: Interactive prompt**
   ```bash
   dotnet run
   # You'll be prompted to enter your API key
   ```

3. **Start chatting**:
   ```
   You: Help me create an ADR for choosing a database
   Assistant: I'd be happy to help you create an ADR for choosing a database...
   ```

## Configuration

The sample shows how to configure Agentix with a custom system prompt directly in the service registration:

```csharp
builder.ConfigureServices(services =>
{
    services.AddAgentixCore(options =>
    {
        options.SystemPrompt = @"You are an expert Architecture Decision Record (ADR) assistant...";
        options.EnableCostTracking = true;
    })
    .AddClaudeProvider(options =>
    {
        options.ApiKey = claudeApiKey;
        options.DefaultModel = "claude-3-haiku-20240307";
        options.Temperature = 0.7f;
        options.MaxTokens = 1000;
    })
    .AddConsoleChannel();
});
```

## What's Happening

1. **Configuration**: The framework is configured with Claude as the AI provider and Console as the communication channel
2. **System Prompt**: The AI is configured as an ADR specialist with specific knowledge and behavior
3. **Orchestration**: The `IAgentixOrchestrator` coordinates between the channel and provider
4. **Message Flow**: User input → Console Channel → Orchestrator → Claude Provider (with system prompt) → Response back to user

## System Prompt Features

This sample demonstrates how to configure a single system prompt that defines the AI's role and behavior. The system prompt:

- Makes the AI an expert in Architecture Decision Records
- Provides specific knowledge domains and capabilities
- Sets the tone and approach for interactions

## Next Steps

- Try the [Web API sample](../Agentix.Sample.Web/) for HTTP-based interactions
- Explore the [Multi-Provider sample](../Agentix.Sample.MultiProvider/) to see provider switching
- Check out [Channel examples](../Agentix.Sample.Slack/) for Slack integration