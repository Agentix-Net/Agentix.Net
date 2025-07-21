namespace Agentix.Core.Interfaces;

/// <summary>
/// Abstraction for storing and retrieving conversation contexts
/// </summary>
public interface IContextStore
{
    Task<IConversationContext?> GetContextAsync(string contextId);
    Task<IConversationContext> CreateContextAsync(string contextId, string userId, string channelId, string channelType);
    Task SaveContextAsync(IConversationContext context);
    Task DeleteContextAsync(string contextId);
    Task<IEnumerable<string>> GetUserContextsAsync(string userId);
    Task CleanupExpiredContextsAsync();
} 