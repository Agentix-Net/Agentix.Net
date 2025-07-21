using System.Collections.Concurrent;
using System.Text.Json;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;

namespace Agentix.Context.InMemory;

/// <summary>
/// Internal in-memory implementation of conversation context interface.
/// Provides thread-safe storage for conversation messages, key-value data, and tool results.
/// </summary>
/// <remarks>
/// This implementation uses concurrent collections to ensure thread safety and automatically
/// manages memory by limiting the number of stored messages and tool results. It includes
/// automatic expiration handling and data cleanup mechanisms.
/// </remarks>
internal class InMemoryConversationContext : IConversationContext
{
    private readonly ConcurrentQueue<ContextMessage> _messages = new();
    private readonly ConcurrentDictionary<string, ContextData> _data = new();
    private readonly ConcurrentQueue<ToolResult> _toolResults = new();
    private readonly object _lockObject = new();

    public string ContextId { get; }
    public string UserId { get; }
    public string ChannelId { get; }
    public string ChannelType { get; }
    public DateTime CreatedAt { get; }
    public DateTime LastActivity { get; private set; }
    public DateTime ExpiresAt { get; private set; }

    public InMemoryConversationContext(string contextId, string userId, string channelId, string channelType, TimeSpan? expiration = null)
    {
        ContextId = contextId;
        UserId = userId;
        ChannelId = channelId;
        ChannelType = channelType;
        CreatedAt = DateTime.UtcNow;
        LastActivity = DateTime.UtcNow;
        ExpiresAt = DateTime.UtcNow.Add(expiration ?? TimeSpan.FromHours(4));
    }

    public Task<IEnumerable<ContextMessage>> GetMessagesAsync(int maxCount = 10)
    {
        var messages = _messages.ToArray()
            .OrderBy(m => m.Timestamp)
            .TakeLast(maxCount);
        
        return Task.FromResult(messages);
    }

    public Task AddMessageAsync(ContextMessage message)
    {
        _messages.Enqueue(message);
        
        // Keep only the most recent messages to prevent memory bloat
        while (_messages.Count > 100)
        {
            _messages.TryDequeue(out _);
        }
        
        return RefreshActivityAsync();
    }

    public Task ClearMessagesAsync()
    {
        while (_messages.TryDequeue(out _)) { }
        return Task.CompletedTask;
    }

    public Task<T?> GetAsync<T>(string key) where T : class
    {
        if (_data.TryGetValue(key, out var contextData))
        {
            // Check expiration
            if (contextData.ExpiresAt.HasValue && contextData.ExpiresAt <= DateTime.UtcNow)
            {
                _data.TryRemove(key, out _);
                return Task.FromResult<T?>(null);
            }

            // Handle deserialization
            if (contextData.Value is T directValue)
            {
                return Task.FromResult<T?>(directValue);
            }

            if (contextData.Value is JsonElement jsonElement)
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(jsonElement.GetRawText());
                    return Task.FromResult(deserialized);
                }
                catch
                {
                    return Task.FromResult<T?>(null);
                }
            }

            if (contextData.Value is string jsonString)
            {
                try
                {
                    var deserialized = JsonSerializer.Deserialize<T>(jsonString);
                    return Task.FromResult(deserialized);
                }
                catch
                {
                    return Task.FromResult<T?>(null);
                }
            }
        }

        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var contextData = new ContextData
        {
            Key = key,
            Value = value,
            Type = typeof(T).FullName ?? string.Empty,
            ExpiresAt = expiration.HasValue ? DateTime.UtcNow.Add(expiration.Value) : null
        };

        _data.AddOrUpdate(key, contextData, (k, v) => contextData);
        return RefreshActivityAsync();
    }

    public Task RemoveAsync(string key)
    {
        _data.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetKeysAsync()
    {
        // Clean up expired keys first
        var expiredKeys = _data.Where(kvp => kvp.Value.ExpiresAt.HasValue && kvp.Value.ExpiresAt <= DateTime.UtcNow)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var expiredKey in expiredKeys)
        {
            _data.TryRemove(expiredKey, out _);
        }

        return Task.FromResult(_data.Keys.AsEnumerable());
    }

    public Task<IEnumerable<ToolResult>> GetRecentToolResultsAsync(string? toolName = null, int maxCount = 5)
    {
        var results = _toolResults.ToArray()
            .Where(tr => string.IsNullOrEmpty(toolName) || tr.ToolName == toolName)
            .OrderByDescending(tr => tr.Timestamp)
            .Take(maxCount);

        return Task.FromResult(results);
    }

    public Task AddToolResultAsync(ToolResult result)
    {
        _toolResults.Enqueue(result);
        
        // Keep only recent tool results to prevent memory bloat
        while (_toolResults.Count > 50)
        {
            _toolResults.TryDequeue(out _);
        }
        
        return RefreshActivityAsync();
    }

    public Task ExtendExpirationAsync(TimeSpan extension)
    {
        lock (_lockObject)
        {
            ExpiresAt = ExpiresAt.Add(extension);
        }
        return Task.CompletedTask;
    }

    public Task RefreshActivityAsync()
    {
        lock (_lockObject)
        {
            LastActivity = DateTime.UtcNow;
        }
        return Task.CompletedTask;
    }

    public Task<bool> IsExpiredAsync()
    {
        return Task.FromResult(DateTime.UtcNow > ExpiresAt);
    }

    public Task ClearAsync()
    {
        while (_messages.TryDequeue(out _)) { }
        while (_toolResults.TryDequeue(out _)) { }
        _data.Clear();
        return Task.CompletedTask;
    }
} 