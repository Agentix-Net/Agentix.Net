# Agentix Channels

This directory contains channel adapter implementations that allow Agentix to communicate through different platforms and interfaces.

## Existing Channels

- **Agentix.Channels.Console** - Console/terminal interface for command-line applications

## Adding a New Channel

To add a new channel adapter (e.g., Slack, Teams, Web API):

1. Create a new project directory following the naming convention: `Agentix.Channels.{ChannelName}`
2. Implement the `IChannelAdapter` interface from `Agentix.Core`
3. Add your project to the solution file
4. Add project reference to `Agentix.Core`

### Example Project Structure

```
src/Channels/Agentix.Channels.Slack/
├── Agentix.Channels.Slack.csproj
├── SlackChannelAdapter.cs
├── Models/
│   ├── SlackMessage.cs
│   └── SlackOptions.cs
└── README.md
```

### Example Implementation

```csharp
public class SlackChannelAdapter : IChannelAdapter
{
    public string Name => "slack";
    public string ChannelType => "slack";
    public bool IsRunning { get; private set; }
    
    public bool SupportsRichContent => true;
    public bool SupportsFileUploads => true;
    public bool SupportsInteractiveElements => true;
    
    // Implement interface methods...
}
```

## Planned Channels

- **Agentix.Channels.Slack** - Slack bot integration
- **Agentix.Channels.Teams** - Microsoft Teams bot integration  
- **Agentix.Channels.WebApi** - HTTP REST API interface
- **Agentix.Channels.SignalR** - Real-time web interface
- **Agentix.Channels.Discord** - Discord bot integration 