# Agentix Slack Channel Adapter

The Slack channel adapter enables your Agentix AI agents to interact with users through Slack, providing a seamless conversational experience in your workspace.

## Features

- 🔗 **Dual Connection Modes**: Choose between Webhooks (production) or Socket Mode (development)
- 💬 **Direct Messages**: Responds to direct messages to your bot
- 🏷️ **Mention Support**: Responds when your bot is mentioned in channels
- 🧵 **Threading**: Uses Slack threads to keep conversations organized
- 🔒 **Security**: Verifies webhook signatures for secure communication
- 🔌 **Socket Mode**: Real-time WebSocket connection, perfect for development (no public endpoint needed)
- 🌐 **Webhook Mode**: Traditional HTTP webhooks for production environments
- ⚙️ **Configurable**: Extensive configuration options for different use cases
- 📊 **Monitoring**: Optional usage tracking and cost monitoring
- 🔄 **Auto-Reconnection**: Automatic reconnection handling for Socket Mode

## Connection Modes

### Webhook Mode (Production)
- ✅ Stateless and horizontally scalable
- ✅ Works well with load balancers
- ✅ Slack handles delivery guarantees
- ❌ Requires public HTTPS endpoint
- ❌ Complex local development setup (needs tunneling)

### Socket Mode (Development)
- ✅ No public endpoint required
- ✅ Works behind firewalls and NAT
- ✅ Perfect for local development
- ✅ Lower latency, more real-time
- ❌ Stateful connections (harder to scale horizontally)
- ❌ More complex connection management

## Prerequisites

### Common Requirements
1. **Slack App**: You need to create a Slack app in your workspace
2. **Bot Token**: Your app needs the appropriate OAuth scopes and bot token

### Webhook Mode Requirements
3. **Webhook Endpoint**: A publicly accessible HTTPS URL for Slack to send events
4. **Signing Secret**: For verifying webhook requests

### Socket Mode Requirements  
3. **App-Level Token**: Required for Socket Mode authentication (starts with `xapp-`)

## Quick Start

### 1. Create a Slack App

1. Go to [https://api.slack.com/apps](https://api.slack.com/apps)
2. Click "Create New App" → "From scratch"
3. Give your app a name and select your workspace
4. Click "Create App"

### 2. Configure OAuth & Permissions

1. Go to "OAuth & Permissions" in your app settings
2. Add the following Bot Token Scopes:
   - `chat:write` - Send messages
   - `channels:history` - Read channel messages
   - `groups:history` - Read private channel messages
   - `im:history` - Read direct messages
   - `users:read` - Get user information
   - `channels:read` - Get channel information

3. Install the app to your workspace
4. Copy the "Bot User OAuth Token" (starts with `xoxb-`)

### 3. Configure Connection Method

Choose between Webhook Mode or Socket Mode:

#### For Webhook Mode:
1. Go to "Event Subscriptions" in your app settings
2. Enable events
3. Set Request URL to: `https://your-domain.com:3000/slack/events`
4. Subscribe to the following bot events:
   - `message.channels` - Messages in channels
   - `message.groups` - Messages in private channels
   - `message.im` - Direct messages
5. Save changes
6. Go to "Basic Information" and copy the "Signing Secret"

#### For Socket Mode:
1. Go to "Socket Mode" in your app settings
2. Enable Socket Mode
3. Create an app-level token with `connections:write` scope
4. Copy the app-level token (starts with `xapp-`)
5. Go to "Event Subscriptions" and enable events (URL not required for Socket Mode)
6. Subscribe to the same bot events as above

### 4. Use in Your Application

#### Webhook Mode Example:
```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Slack.Extensions;
using Agentix.Channels.Slack;

services.AddAgentixCore(options => { /* ... */ })
    .AddClaudeProvider(options => { /* ... */ })
    .AddSlackChannel(options =>
    {
        options.Mode = SlackChannelMode.Webhook;
        options.BotToken = "xoxb-your-bot-token";
        options.SigningSecret = "your-signing-secret";
        options.WebhookPort = 3000;
        options.RespondToMentionsOnly = true;
        options.RespondToDirectMessages = true;
        options.UseThreading = true;
    });
```

#### Socket Mode Example:
```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Slack.Extensions;
using Agentix.Channels.Slack;

services.AddAgentixCore(options => { /* ... */ })
    .AddClaudeProvider(options => { /* ... */ })
    .AddSlackChannel(options =>
    {
        options.Mode = SlackChannelMode.SocketMode;
        options.BotToken = "xoxb-your-bot-token";
        options.AppToken = "xapp-your-app-token";
        options.RespondToMentionsOnly = true;
        options.RespondToDirectMessages = true;
        options.UseThreading = true;
        options.AutoReconnect = true;
        options.LogConnectionEvents = true; // For debugging
    });
```

## Configuration Options

### SlackChannelOptions

### Common Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Mode` | `SlackChannelMode` | `Webhook` | Connection mode (Webhook or SocketMode) |
| `BotToken` | `string` | **Required** | Slack bot token (xoxb-...) |
| `RespondToMentionsOnly` | `bool` | `true` | Only respond when bot is mentioned in channels |
| `RespondToDirectMessages` | `bool` | `true` | Respond to direct messages |
| `UseThreading` | `bool` | `true` | Use threads for responses in channels |
| `MaxResponseLength` | `int` | `3900` | Maximum length for responses |
| `ShowMetadata` | `bool` | `false` | Include usage metadata in responses |
| `BotName` | `string?` | `null` | Custom bot display name |
| `BotEmoji` | `string?` | `null` | Custom bot emoji icon |
| `AllowedChannels` | `List<string>` | Empty | Restrict bot to specific channels |
| `AllowedUsers` | `List<string>` | Empty | Restrict bot to specific users |
| `LogMessages` | `bool` | `false` | Log incoming messages for debugging |
| `ApiTimeoutSeconds` | `int` | `30` | Timeout for Slack API calls |

### Webhook Mode Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `SigningSecret` | `string` | **Required** | Slack signing secret for webhook verification |
| `WebhookPort` | `int` | `3000` | Port for the webhook server |
| `WebhookPath` | `string` | `/slack/events` | Path for webhook endpoint |

### Socket Mode Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `AppToken` | `string` | **Required** | App-level token for Socket Mode (xapp-...) |
| `HeartbeatInterval` | `TimeSpan` | `30s` | Interval for sending ping messages |
| `ReconnectTimeout` | `TimeSpan` | `30s` | Time to wait between reconnection attempts |
| `MaxReconnectAttempts` | `int` | `10` | Maximum reconnection attempts before giving up |
| `AutoReconnect` | `bool` | `true` | Automatically reconnect when connection is lost |
| `LogConnectionEvents` | `bool` | `false` | Log WebSocket connection events for debugging |

## Environment Variables

You can use environment variables for configuration:

### For Webhook Mode:
```bash
export SLACK_MODE="webhook"
export SLACK_BOT_TOKEN="xoxb-your-bot-token"
export SLACK_SIGNING_SECRET="your-signing-secret"
export CLAUDE_API_KEY="your-claude-api-key"
```

### For Socket Mode:
```bash
export SLACK_MODE="socket"
export SLACK_BOT_TOKEN="xoxb-your-bot-token"  
export SLACK_APP_TOKEN="xapp-your-app-token"
export CLAUDE_API_KEY="your-claude-api-key"
```

## Deployment

### Development

#### Option 1: Socket Mode (Recommended for Development)
Socket Mode is perfect for local development as it doesn't require a public endpoint:

```bash
# Set environment variables
export SLACK_MODE="socket"
export SLACK_BOT_TOKEN="xoxb-your-bot-token"
export SLACK_APP_TOKEN="xapp-your-app-token"
export CLAUDE_API_KEY="your-claude-api-key"

# Run your application
dotnet run
```

#### Option 2: Webhook Mode with ngrok
If you prefer webhook mode for development, use ngrok to expose your local server:

```bash
# Install ngrok
npm install -g ngrok

# Expose local port
ngrok http 3000

# Use the provided URL as your webhook endpoint
# Example: https://abc123.ngrok.io/slack/events
```

### Production

Deploy your application to a cloud service with:
- Public HTTPS endpoint
- Port 3000 accessible (or configure different port)
- Environment variables configured

Popular deployment options:
- Azure App Service
- AWS Elastic Beanstalk
- Google Cloud Run
- Docker containers
- Railway, Render, or similar platforms

## Security

The Slack channel adapter includes several security features:

1. **Signature Verification**: All webhook requests are verified using HMAC-SHA256
2. **Timestamp Validation**: Prevents replay attacks (5-minute window)
3. **User/Channel Filtering**: Optional allowlists for users and channels
4. **Bot Loop Prevention**: Ignores messages from bots and itself

## Troubleshooting

### Common Issues

**"Invalid Slack signature" warnings**
- Verify your signing secret is correct
- Ensure your webhook URL is accessible
- Check that Slack can reach your endpoint

**Bot doesn't respond to messages**
- Verify bot token has correct scopes
- Check if `RespondToMentionsOnly` is true (requires @mention)
- Ensure event subscriptions are configured correctly

**Messages are truncated**
- Slack has a 4000 character limit for messages
- Adjust `MaxResponseLength` if needed
- Consider breaking long responses into multiple messages

### Debug Mode

Enable debug logging:

```csharp
.AddSlackChannel(options =>
{
    options.ShowMetadata = true;
    options.LogMessages = true;
    // ... other options
});
```

### Health Check

The channel adapter provides a health check endpoint:
- `GET http://your-domain:3000/health` → Returns "OK"

## Examples

See the `samples/Agentix.Sample.Slack` project for a complete working example.

## License

This project is licensed under the MIT License - see the LICENSE file for details. 