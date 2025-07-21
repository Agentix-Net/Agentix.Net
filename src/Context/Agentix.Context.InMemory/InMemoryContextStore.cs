using System.Collections.Concurrent;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agentix.Context.InMemory;

/// <summary>
/// In-memory implementation of context store
/// </summary>
public class InMemoryContextStore : IContextStore
{
    private readonly ConcurrentDictionary<string, InMemoryConversationContext> _contexts = new();
    private readonly IContextResolver _contextResolver;
    private readonly ILogger<InMemoryContextStore> _logger;
    private readonly Timer _cleanupTimer;

    public InMemoryContextStore(IContextResolver contextResolver, ILogger<InMemoryContextStore> logger)
    {
        _contextResolver = contextResolver;
        _logger = logger;
        
        // Setup cleanup timer to run every 5 minutes
        _cleanupTimer = new Timer(CleanupExpiredContexts, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public async Task<IConversationContext?> GetContextAsync(string contextId)
    {
        if (_contexts.TryGetValue(contextId, out var context) && !await context.IsExpiredAsync())
        {
            await context.RefreshActivityAsync();
            return context;
        }
        else if (_contexts.ContainsKey(contextId))
        {
            // Remove expired context
            _contexts.TryRemove(contextId, out _);
            _logger.LogDebug("Removed expired context: {ContextId}", contextId);
        }
        
        return null;
    }

    public Task<IConversationContext> CreateContextAsync(string contextId, string userId, string channelId, string channelType)
    {
        var expiration = _contextResolver.GetDefaultExpiration(channelType);
        var context = new InMemoryConversationContext(contextId, userId, channelId, channelType, expiration);
        
        _contexts.TryAdd(contextId, context);
        
        _logger.LogDebug("Created new context: {ContextId} for user {UserId} in {ChannelType}", 
                        contextId, userId, channelType);
        
        return Task.FromResult<IConversationContext>(context);
    }

    public Task SaveContextAsync(IConversationContext context)
    {
        // In memory store doesn't need explicit saving since changes are made directly to objects
        // This method exists for interface compatibility with other stores (Redis, Database)
        return Task.CompletedTask;
    }

    public Task DeleteContextAsync(string contextId)
    {
        if (_contexts.TryRemove(contextId, out var context))
        {
            _logger.LogDebug("Deleted context: {ContextId}", contextId);
        }
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetUserContextsAsync(string userId)
    {
        var userContexts = _contexts.Values
            .Where(c => c.UserId == userId)
            .Select(c => c.ContextId);
        
        return Task.FromResult(userContexts);
    }

    public async Task CleanupExpiredContextsAsync()
    {
        var expiredContexts = new List<string>();
        
        foreach (var kvp in _contexts)
        {
            if (await kvp.Value.IsExpiredAsync())
            {
                expiredContexts.Add(kvp.Key);
            }
        }
        
        foreach (var contextId in expiredContexts)
        {
            _contexts.TryRemove(contextId, out _);
        }
        
        if (expiredContexts.Count > 0)
        {
            _logger.LogInformation("Cleaned up {Count} expired contexts", expiredContexts.Count);
        }
    }

    private async void CleanupExpiredContexts(object? state)
    {
        try
        {
            await CleanupExpiredContextsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during context cleanup");
        }
    }

    public void Dispose()
    {
        _cleanupTimer?.Dispose();
    }
} 