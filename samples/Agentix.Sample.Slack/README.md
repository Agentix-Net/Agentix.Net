# Agentix Slack Sample

This sample demonstrates how to create a Slack bot using the Agentix framework with Claude AI integration.

## Features

- **Slack Bot Integration**: Responds to direct messages and mentions in channels
- **Dual Connection Modes**: Supports both Webhook and Socket Mode
- **Threading Support**: Keeps conversations organized in Slack threads
- **Real-time Responses**: Live interaction with Claude AI
- **Cost Tracking**: Optional usage and cost monitoring
- **Secure Communication**: Webhook signature verification

## Prerequisites

- .NET 8.0 or later
- Claude API key from [Anthropic Console](https://console.anthropic.com/)
- Slack workspace with admin permissions to create apps

## Setup Instructions

### 1. Create a Slack App

1. Go to [https://api.slack.com/apps](https://api.slack.com/apps)
2. Click "Create New App" → "From scratch"
3. Give your app a name (e.g., "Agentix AI Bot") and select your workspace
4. Click "Create App"

### 2. Configure OAuth & Permissions

1. In your app settings, go to "OAuth & Permissions"
2. Scroll down to "Bot Token Scopes" and add these scopes:
   - `chat:write` - Send messages
   - `channels:history` - Read channel messages (for mentions)
   - `groups:history` - Read private channel messages
   - `im:history` - Read direct messages
   - `users:read` - Get user information
   - `channels:read` - Get channel information

3. Click "Install App" at the top of the page
4. Copy the "Bot User OAuth Token" (starts with `xoxb-`)

### 3. Choose Connection Mode

#### Option A: Socket Mode (Recommended for Development)

Socket Mode is perfect for local development as it doesn't require a public endpoint.

1. Go to "Socket Mode" in your app settings
2. Enable Socket Mode
3. Create an app-level token with `connections:write` scope
4. Copy the app-level token (starts with `xapp-`)
5. Go to "Event Subscriptions" and enable events
6. Subscribe to these bot events:
   - `message.channels` - Messages in channels
   - `message.groups` - Messages in private channels  
   - `message.im` - Direct messages

#### Option B: Webhook Mode (For Production)

1. Go to "Event Subscriptions" in your app settings
2. Enable events
3. Set Request URL to: `https://your-domain.com/slack/events`
4. Subscribe to the same bot events as above
5. Go to "Basic Information" and copy the "Signing Secret"

### 4. Get Your Claude API Key

1. Visit [Anthropic Console](https://console.anthropic.com/)
2. Create an account or sign in
3. Generate an API key
4. Ensure you have sufficient credits

## Running the Sample

### Socket Mode (Local Development)

```bash
# Clone and navigate to the sample
cd samples/Agentix.Sample.Slack

# Set environment variables
export SLACK_BOT_TOKEN="xoxb-your-bot-token"
export SLACK_APP_TOKEN="xapp-your-app-token"
export CLAUDE_API_KEY="your-claude-api-key"

# Run the application
dotnet run
```

### Webhook Mode (Production)

```bash
# Set environment variables
export SLACK_BOT_TOKEN="xoxb-your-bot-token"
export SLACK_SIGNING_SECRET="your-signing-secret"
export CLAUDE_API_KEY="your-claude-api-key"

# Run on the specified port (default: 3000)
dotnet run
```

### Using Configuration File

Create an `appsettings.json` file:

```json
{
  "Slack": {
    "Mode": "SocketMode",
    "BotToken": "xoxb-your-bot-token",
    "AppToken": "xapp-your-app-token",
    "RespondToMentionsOnly": true,
    "RespondToDirectMessages": true,
    "UseThreading": true,
    "ShowMetadata": false
  },
  "Claude": {
    "ApiKey": "your-claude-api-key",
    "DefaultModel": "claude-3-haiku-20240307",
    "Temperature": 0.7,
    "MaxTokens": 500
  }
}
```

Then run:

```bash
dotnet run
```

## Usage Examples

### In Slack Direct Messages

```
You: Hello, how can you help me?

Bot: Hi! I'm an AI assistant powered by Claude. I can help you with:
- Answering questions
- Code assistance  
- Writing and editing
- Research and analysis
- Problem solving

What would you like to work on today?
```

### In Slack Channels (with @mentions)

```
You: @AgentixBot explain async/await in C#

Bot: Async/await in C# is a powerful pattern for handling asynchronous operations...
[Bot responds in a thread to keep the channel organized]
```

### Code Analysis

```
You: Can you review this C# code for best practices?

public class UserService
{
    public User GetUser(int id)
    {
        // Some code here
    }
}

Bot: Here's my analysis of your UserService class:

✅ **Good practices:**
- Clear, descriptive class name
- Simple method signature

⚠️ **Improvements to consider:**
1. Make the method async for better scalability
2. Add error handling for invalid IDs
3. Consider dependency injection for data access

Here's a refactored version:
[Bot provides improved code example]
```

## Configuration Options

The sample demonstrates various configuration patterns:

### Basic Configuration

```csharp
services.AddAgentixCore()
    .AddClaudeProvider(options =>
    {
        options.ApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        options.DefaultModel = "claude-3-haiku-20240307"; // Fast for chat
        options.MaxTokens = 500; // Shorter responses for Slack
    })
    .AddSlackChannel(options =>
    {
        options.Mode = SlackChannelMode.SocketMode;
        options.BotToken = Environment.GetEnvironmentVariable("SLACK_BOT_TOKEN");
        options.AppToken = Environment.GetEnvironmentVariable("SLACK_APP_TOKEN");
        options.RespondToMentionsOnly = true;
        options.RespondToDirectMessages = true;
        options.UseThreading = true;
    });
```

### Advanced Configuration

```csharp
services.AddAgentixCore(options =>
{
    options.SystemPrompt = @"You are a helpful Slack assistant for a software development team. 
        Be concise but helpful. Use emojis appropriately for Slack communication.";
})
.AddClaudeProvider(options =>
{
    options.ApiKey = configuration["Claude:ApiKey"];
    options.DefaultModel = "claude-3-sonnet-20241022";
    options.Temperature = 0.7f;
    options.MaxTokens = 1000;
})
.AddSlackChannel(options =>
{
    options.Mode = SlackChannelMode.SocketMode;
    options.BotToken = configuration["Slack:BotToken"];
    options.AppToken = configuration["Slack:AppToken"];
    
    // Behavior settings
    options.RespondToMentionsOnly = true;
    options.RespondToDirectMessages = true;
    options.UseThreading = true;
    
    // Optional: Restrict to specific channels
    options.AllowedChannels = new List<string> { "general", "dev-team" };
    
    // Optional: Show usage metadata
    options.ShowMetadata = true;
    
    // Customization
    options.BotName = "DevBot";
    options.BotEmoji = ":robot_face:";
});
```

## Bot Behavior

### Response Patterns

**Direct Messages**: Bot always responds to DMs

**Channel Mentions**: Bot responds when mentioned with `@BotName`

**Threading**: Responses are posted in threads to keep channels organized

**Error Handling**: Graceful error messages with retry suggestions

### Example Interactions

**Development Help**:
```
@AgentixBot what's the difference between IEnumerable and IList?

Bot: Great question! Here are the key differences:

📋 **IEnumerable<T>**
- Read-only forward iteration
- Deferred execution (LINQ)
- Lower memory usage
- Best for: Reading data, LINQ queries

📝 **IList<T>**  
- Read/write access by index
- Add/remove items
- Random access
- Best for: Collections you need to modify

Use IEnumerable when you only need to iterate, IList when you need to modify! 🎯
```

**Code Review**:
```
@AgentixBot can you review this LINQ query?

var users = db.Users.Where(u => u.IsActive).ToList().Where(u => u.Age > 18);

Bot: I see a performance issue! 🚨

❌ **Current**: Loads ALL active users into memory, then filters
✅ **Better**: Filter in the database

```csharp
var users = db.Users.Where(u => u.IsActive && u.Age > 18).ToList();
```

This reduces memory usage and database load! 📈
```

## Deployment

### Local Development (Socket Mode)

Socket Mode is perfect for development - no public endpoint needed:

```bash
# Set environment variables
export SLACK_BOT_TOKEN="xoxb-your-token"
export SLACK_APP_TOKEN="xapp-your-token" 
export CLAUDE_API_KEY="your-claude-key"

# Run locally
dotnet run

# Bot connects via WebSocket - no tunneling required!
```

### Production Deployment (Webhook Mode)

For production, deploy to a cloud service with HTTPS:

```bash
# Environment variables for production
SLACK_BOT_TOKEN=xoxb-your-token
SLACK_SIGNING_SECRET=your-signing-secret
CLAUDE_API_KEY=your-claude-key
ASPNETCORE_URLS=https://+:443;http://+:80
```

**Deployment Options**:
- Azure App Service
- AWS Elastic Beanstalk  
- Google Cloud Run
- Docker containers
- Railway, Render, or similar

### Using ngrok for Testing Webhooks

If you want to test webhook mode locally:

```bash
# Install ngrok
npm install -g ngrok

# Expose local port 3000
ngrok http 3000

# Use the HTTPS URL in your Slack app's Event Subscriptions
# Example: https://abc123.ngrok.io/slack/events
```

## Troubleshooting

### Common Issues

**"url_verification failed"**
- Check that your webhook URL is publicly accessible
- Ensure the endpoint is `/slack/events`
- Verify SSL certificate is valid

**Bot doesn't respond to messages**
- Check if `RespondToMentionsOnly` is true (requires @mention)
- Verify OAuth scopes include message reading permissions
- Check bot is added to the channel where you're testing

**"Invalid signing secret" errors**
- Verify your signing secret is correct in Slack app settings
- Ensure you're using webhook mode configuration
- Check that requests are coming from Slack

**WebSocket connection issues (Socket Mode)**
- Verify app token has `connections:write` scope
- Check that Socket Mode is enabled in your Slack app
- Ensure firewall allows outbound WebSocket connections

### Debug Mode

Enable debug logging:

```csharp
services.AddSlackChannel(options =>
{
    options.LogMessages = true;
    options.LogConnectionEvents = true; // For Socket Mode
    options.ShowMetadata = true;
});
```

### Health Check

Test if the bot is working:

```bash
# For webhook mode
curl https://your-domain.com/health

# Should return "OK"
```

## Code Structure

The sample includes:

- `Program.cs` - Main application setup and configuration
- Configuration binding from `appsettings.json`
- Environment variable support
- Comprehensive error handling
- Logging configuration

## Advanced Features

### Custom System Prompts

```csharp
services.AddAgentixCore(options =>
{
    options.SystemPrompt = @"You are SlackBot, a helpful AI assistant for the engineering team.

Guidelines:
- Keep responses concise for chat format
- Use appropriate emojis 
- Format code with triple backticks
- Be friendly but professional
- Provide actionable advice";
});
```

### Channel Restrictions

```csharp
services.AddSlackChannel(options =>
{
    // Only respond in specific channels
    options.AllowedChannels = new List<string> 
    { 
        "general", 
        "engineering", 
        "ai-discussions" 
    };
    
    // Only respond to specific users (optional)
    options.AllowedUsers = new List<string> 
    { 
        "john.doe", 
        "jane.smith" 
    };
});
```

### Cost Monitoring

```csharp
services.AddClaudeProvider(options =>
{
    options.EnableCostTracking = true;
    options.MaxCostPerRequest = 0.25m; // Limit costs
});

services.AddSlackChannel(options =>
{
    options.ShowMetadata = true; // Shows cost info
});
```

## Related Documentation

- [Slack Channel Documentation](../../src/Channels/Agentix.Channels.Slack/README.md)
- [Claude Provider Documentation](../../src/Providers/Agentix.Providers.Claude/README.md)
- [Core Framework Documentation](../../src/Core/Agentix.Core/README.md)

## License

This project is licensed under the MIT License. 