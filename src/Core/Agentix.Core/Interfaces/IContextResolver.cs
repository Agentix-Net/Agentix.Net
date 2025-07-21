using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Resolves context boundaries and lifecycle for different channels.
/// Determines how conversation contexts are identified, managed, and configured across different communication platforms.
/// </summary>
public interface IContextResolver
{
    /// <summary>
    /// Resolves the context identifier for a given incoming message.
    /// This determines which conversation context the message belongs to.
    /// </summary>
    /// <param name="message">The incoming message to resolve context for.</param>
    /// <returns>A unique context identifier that groups related messages together.</returns>
    string ResolveContextId(IncomingMessage message);
    
    /// <summary>
    /// Gets the default expiration time for contexts in the specified channel type.
    /// </summary>
    /// <param name="channelType">The type of channel (e.g., "slack", "console", "teams").</param>
    /// <returns>A <see cref="TimeSpan"/> representing how long contexts should be kept before expiring.</returns>
    TimeSpan GetDefaultExpiration(string channelType);
    
    /// <summary>
    /// Gets the maximum number of messages to keep in history for the specified channel type.
    /// </summary>
    /// <param name="channelType">The type of channel (e.g., "slack", "console", "teams").</param>
    /// <returns>The maximum number of messages to retain in the conversation history.</returns>
    int GetMaxMessageHistory(string channelType);
    
    /// <summary>
    /// Determines whether a new conversation context should be created for the given message.
    /// This is typically used for explicit user commands to start a new conversation.
    /// </summary>
    /// <param name="message">The incoming message to evaluate.</param>
    /// <returns>True if a new context should be created, clearing any existing conversation history; otherwise, false.</returns>
    bool ShouldCreateNewContext(IncomingMessage message);
} 