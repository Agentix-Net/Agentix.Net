using System.Text.RegularExpressions;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;

namespace Agentix.Core.Services.Context;

/// <summary>
/// Default implementation of context resolver with channel-specific strategies
/// </summary>
public class DefaultContextResolver : IContextResolver
{
    public string ResolveContextId(IncomingMessage message)
    {
        return message.Channel switch
        {
            "slack" => $"slack:{message.ChannelId}:{message.UserId}",
            "teams" => message.Metadata.ContainsKey("conversationId") 
                      ? $"teams:{message.Metadata["conversationId"]}"
                      : $"teams:{message.ChannelId}:{message.UserId}",
            "webapi" => message.Metadata.ContainsKey("sessionId")
                       ? $"web:{message.Metadata["sessionId"]}"
                       : $"web:{message.UserId}",
            "console" => $"console:{Environment.MachineName}:{message.UserId}",
            _ => $"default:{message.Channel}:{message.UserId}"
        };
    }

    public TimeSpan GetDefaultExpiration(string channelType)
    {
        return channelType switch
        {
            "slack" => TimeSpan.FromHours(4),    // Slack conversations are shorter
            "teams" => TimeSpan.FromHours(8),    // Teams conversations can be longer
            "webapi" => TimeSpan.FromHours(1),   // Web sessions shorter by default
            "console" => TimeSpan.FromDays(1),   // Console sessions can be longer
            _ => TimeSpan.FromHours(2)
        };
    }

    public int GetMaxMessageHistory(string channelType)
    {
        return channelType switch
        {
            "slack" => 15,   // Slack moves fast
            "teams" => 25,   // Teams conversations can be more detailed
            "webapi" => 10,  // Keep web light
            "console" => 50, // Console users might have longer sessions
            _ => 20
        };
    }

    public bool ShouldCreateNewContext(IncomingMessage message)
    {
        // Create new context for explicit "new" commands
        var content = message.Content.ToLowerInvariant();
        return content.Contains("new conversation") || 
               content.Contains("start over") ||
               content.Contains("clear context") ||
               content.Contains("reset conversation");
    }
} 