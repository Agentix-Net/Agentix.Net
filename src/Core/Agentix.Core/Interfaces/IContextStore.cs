namespace Agentix.Core.Interfaces;

/// <summary>
/// Abstraction for storing and retrieving conversation contexts.
/// Provides persistence and management of conversation state across the application lifecycle.
/// </summary>
public interface IContextStore
{
    /// <summary>
    /// Retrieves an existing conversation context by its identifier.
    /// </summary>
    /// <param name="contextId">The unique identifier of the context to retrieve.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the conversation context if found, or null if not found.</returns>
    Task<IConversationContext?> GetContextAsync(string contextId);
    
    /// <summary>
    /// Creates a new conversation context with the specified parameters.
    /// </summary>
    /// <param name="contextId">The unique identifier for the new context.</param>
    /// <param name="userId">The user identifier associated with this context.</param>
    /// <param name="channelId">The channel identifier where the conversation takes place.</param>
    /// <param name="channelType">The type of channel (e.g., "slack", "console", "teams").</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the newly created conversation context.</returns>
    Task<IConversationContext> CreateContextAsync(string contextId, string userId, string channelId, string channelType);
    
    /// <summary>
    /// Saves changes made to a conversation context back to the store.
    /// </summary>
    /// <param name="context">The conversation context to save.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveContextAsync(IConversationContext context);
    
    /// <summary>
    /// Deletes a conversation context from the store.
    /// </summary>
    /// <param name="contextId">The unique identifier of the context to delete.</param>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    Task DeleteContextAsync(string contextId);
    
    /// <summary>
    /// Retrieves all context identifiers associated with a specific user.
    /// </summary>
    /// <param name="userId">The user identifier to find contexts for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of context identifiers for the user.</returns>
    Task<IEnumerable<string>> GetUserContextsAsync(string userId);
    
    /// <summary>
    /// Removes expired conversation contexts from the store to free up resources.
    /// </summary>
    /// <returns>A task that represents the asynchronous cleanup operation.</returns>
    Task CleanupExpiredContextsAsync();
} 