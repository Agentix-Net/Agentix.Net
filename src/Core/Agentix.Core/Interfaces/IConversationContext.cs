using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Represents a conversation context that maintains state across multiple messages
/// </summary>
public interface IConversationContext
{
    string ContextId { get; }
    string UserId { get; }
    string ChannelId { get; }
    string ChannelType { get; }
    DateTime CreatedAt { get; }
    DateTime LastActivity { get; }
    DateTime ExpiresAt { get; }
    
    // Message history management
    Task<IEnumerable<ContextMessage>> GetMessagesAsync(int maxCount = 10);
    Task AddMessageAsync(ContextMessage message);
    Task ClearMessagesAsync();
    
    // Key-value context data storage
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<IEnumerable<string>> GetKeysAsync();
    
    // Tool results from current conversation
    Task<IEnumerable<ToolResult>> GetRecentToolResultsAsync(string? toolName = null, int maxCount = 5);
    Task AddToolResultAsync(ToolResult result);
    
    // Context lifecycle
    Task ExtendExpirationAsync(TimeSpan extension);
    Task RefreshActivityAsync();
    Task<bool> IsExpiredAsync();
    Task ClearAsync();
} 