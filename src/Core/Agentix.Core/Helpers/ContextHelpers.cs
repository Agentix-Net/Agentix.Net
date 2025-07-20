using Agentix.Core.Models;

namespace Agentix.Core.Helpers;

public static class ContextHelpers
{
    /// <summary>
    /// Generates a simple context ID for message processing.
    /// This will be enhanced with proper context management in future versions.
    /// </summary>
    /// <param name="message">The incoming message</param>
    /// <returns>A context ID string</returns>
    public static string GenerateContextId(IncomingMessage message)
    {
        return $"{message.Channel}:{message.ChannelId}:{message.UserId}";
    }
} 