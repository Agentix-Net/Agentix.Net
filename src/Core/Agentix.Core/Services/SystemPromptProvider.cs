using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

public class SystemPromptProvider : ISystemPromptProvider
{
    private readonly ILogger<SystemPromptProvider> _logger;
    private readonly SystemPromptConfiguration _configuration;
    private readonly object _lock = new object();

    public SystemPromptProvider(ILogger<SystemPromptProvider> logger)
    {
        _logger = logger;
        _configuration = new SystemPromptConfiguration();
    }

    public string GetSystemPrompt(string? channelType = null, string? userId = null, string? contextId = null)
    {
        lock (_lock)
        {
            // Priority order: User-specific > Channel-specific > Default
            
            // 1. Check for user-specific prompt
            if (!string.IsNullOrEmpty(userId) && _configuration.UserPrompts.TryGetValue(userId, out var userPrompt))
            {
                _logger.LogDebug("Using user-specific system prompt for user {UserId}", userId);
                return userPrompt;
            }
            
            // 2. Check for channel-specific prompt
            if (!string.IsNullOrEmpty(channelType) && _configuration.ChannelPrompts.TryGetValue(channelType, out var channelPrompt))
            {
                _logger.LogDebug("Using channel-specific system prompt for channel {ChannelType}", channelType);
                return channelPrompt;
            }
            
            // 3. Fall back to default
            _logger.LogDebug("Using default system prompt");
            return _configuration.DefaultSystemPrompt;
        }
    }

    public void SetDefaultSystemPrompt(string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException("System prompt cannot be null or empty", nameof(systemPrompt));
        }

        lock (_lock)
        {
            _configuration.DefaultSystemPrompt = systemPrompt;
            _logger.LogInformation("Default system prompt updated");
        }
    }

    public void SetChannelSystemPrompt(string channelType, string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(channelType))
        {
            throw new ArgumentException("Channel type cannot be null or empty", nameof(channelType));
        }
        
        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException("System prompt cannot be null or empty", nameof(systemPrompt));
        }

        lock (_lock)
        {
            _configuration.ChannelPrompts[channelType] = systemPrompt;
            _logger.LogInformation("System prompt updated for channel type {ChannelType}", channelType);
        }
    }

    public void SetUserSystemPrompt(string userId, string systemPrompt)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }
        
        if (string.IsNullOrWhiteSpace(systemPrompt))
        {
            throw new ArgumentException("System prompt cannot be null or empty", nameof(systemPrompt));
        }

        lock (_lock)
        {
            _configuration.UserPrompts[userId] = systemPrompt;
            _logger.LogInformation("System prompt updated for user {UserId}", userId);
        }
    }

    public void RemoveChannelSystemPrompt(string channelType)
    {
        if (string.IsNullOrWhiteSpace(channelType))
        {
            throw new ArgumentException("Channel type cannot be null or empty", nameof(channelType));
        }

        lock (_lock)
        {
            if (_configuration.ChannelPrompts.Remove(channelType))
            {
                _logger.LogInformation("System prompt removed for channel type {ChannelType}", channelType);
            }
        }
    }

    public void RemoveUserSystemPrompt(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
        }

        lock (_lock)
        {
            if (_configuration.UserPrompts.Remove(userId))
            {
                _logger.LogInformation("System prompt removed for user {UserId}", userId);
            }
        }
    }

    public SystemPromptConfiguration GetConfiguration()
    {
        lock (_lock)
        {
            // Return a copy to prevent external modification
            return new SystemPromptConfiguration
            {
                DefaultSystemPrompt = _configuration.DefaultSystemPrompt,
                ChannelPrompts = new Dictionary<string, string>(_configuration.ChannelPrompts),
                UserPrompts = new Dictionary<string, string>(_configuration.UserPrompts)
            };
        }
    }
} 