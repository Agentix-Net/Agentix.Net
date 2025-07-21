using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Represents a conversation context that maintains state across multiple messages.
/// Provides access to message history, key-value storage, and tool results for a specific conversation.
/// </summary>
public interface IConversationContext
{
    /// <summary>
    /// Gets the unique identifier for this conversation context.
    /// </summary>
    /// <value>A string that uniquely identifies this conversation context.</value>
    string ContextId { get; }
    
    /// <summary>
    /// Gets the user identifier associated with this conversation.
    /// </summary>
    /// <value>A string identifying the user participating in this conversation.</value>
    string UserId { get; }
    
    /// <summary>
    /// Gets the channel identifier where this conversation is taking place.
    /// </summary>
    /// <value>A string identifying the specific channel (e.g., channel ID, chat ID).</value>
    string ChannelId { get; }
    
    /// <summary>
    /// Gets the type of channel where this conversation is taking place.
    /// </summary>
    /// <value>A string representing the channel type (e.g., "slack", "console", "teams").</value>
    string ChannelType { get; }
    
    /// <summary>
    /// Gets the date and time when this conversation context was created.
    /// </summary>
    /// <value>A <see cref="DateTime"/> representing when the context was created.</value>
    DateTime CreatedAt { get; }
    
    /// <summary>
    /// Gets the date and time of the last activity in this conversation.
    /// </summary>
    /// <value>A <see cref="DateTime"/> representing the last message or interaction time.</value>
    DateTime LastActivity { get; }
    
    /// <summary>
    /// Gets the date and time when this conversation context will expire.
    /// </summary>
    /// <value>A <see cref="DateTime"/> representing when the context expires and will be cleaned up.</value>
    DateTime ExpiresAt { get; }
    
    /// <summary>
    /// Retrieves the most recent messages from this conversation.
    /// </summary>
    /// <param name="maxCount">The maximum number of messages to retrieve. Default is 10.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of context messages ordered by timestamp.</returns>
    Task<IEnumerable<ContextMessage>> GetMessagesAsync(int maxCount = 10);
    
    /// <summary>
    /// Adds a new message to this conversation's history.
    /// </summary>
    /// <param name="message">The context message to add to the conversation history.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddMessageAsync(ContextMessage message);
    
    /// <summary>
    /// Clears all messages from this conversation's history.
    /// </summary>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    Task ClearMessagesAsync();
    
    /// <summary>
    /// Retrieves a stored value by key from the conversation's key-value storage.
    /// </summary>
    /// <typeparam name="T">The type of the value to retrieve.</typeparam>
    /// <param name="key">The key to look up in the storage.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the stored value or null if not found.</returns>
    Task<T?> GetAsync<T>(string key) where T : class;
    
    /// <summary>
    /// Stores a value with the specified key in the conversation's key-value storage.
    /// </summary>
    /// <typeparam name="T">The type of the value to store.</typeparam>
    /// <param name="key">The key to store the value under.</param>
    /// <param name="value">The value to store.</param>
    /// <param name="expiration">Optional expiration time for this specific value.</param>
    /// <returns>A task that represents the asynchronous set operation.</returns>
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    
    /// <summary>
    /// Removes a stored value by key from the conversation's key-value storage.
    /// </summary>
    /// <param name="key">The key of the value to remove.</param>
    /// <returns>A task that represents the asynchronous remove operation.</returns>
    Task RemoveAsync(string key);
    
    /// <summary>
    /// Retrieves all keys currently stored in the conversation's key-value storage.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of all stored keys.</returns>
    Task<IEnumerable<string>> GetKeysAsync();
    
    /// <summary>
    /// Retrieves recent tool results from this conversation.
    /// </summary>
    /// <param name="toolName">Optional tool name to filter results. If null, returns results from all tools.</param>
    /// <param name="maxCount">The maximum number of tool results to retrieve. Default is 5.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of tool results ordered by timestamp.</returns>
    Task<IEnumerable<ToolResult>> GetRecentToolResultsAsync(string? toolName = null, int maxCount = 5);
    
    /// <summary>
    /// Adds a tool result to this conversation's history.
    /// </summary>
    /// <param name="result">The tool result to add to the conversation history.</param>
    /// <returns>A task that represents the asynchronous add operation.</returns>
    Task AddToolResultAsync(ToolResult result);
    
    /// <summary>
    /// Extends the expiration time of this conversation context.
    /// </summary>
    /// <param name="extension">The amount of time to extend the expiration by.</param>
    /// <returns>A task that represents the asynchronous extend operation.</returns>
    Task ExtendExpirationAsync(TimeSpan extension);
    
    /// <summary>
    /// Refreshes the last activity timestamp to the current time.
    /// </summary>
    /// <returns>A task that represents the asynchronous refresh operation.</returns>
    Task RefreshActivityAsync();
    
    /// <summary>
    /// Checks whether this conversation context has expired.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the context has expired; otherwise, false.</returns>
    Task<bool> IsExpiredAsync();
    
    /// <summary>
    /// Clears all data from this conversation context, including messages, key-value storage, and tool results.
    /// </summary>
    /// <returns>A task that represents the asynchronous clear operation.</returns>
    Task ClearAsync();
} 