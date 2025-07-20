using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

public interface ISystemPromptProvider
{
    /// <summary>
    /// Gets the system prompt for a specific context
    /// </summary>
    string GetSystemPrompt(string? channelType = null, string? userId = null, string? contextId = null);
    
    /// <summary>
    /// Sets a global default system prompt
    /// </summary>
    void SetDefaultSystemPrompt(string systemPrompt);
    
    /// <summary>
    /// Sets a system prompt for a specific channel type
    /// </summary>
    void SetChannelSystemPrompt(string channelType, string systemPrompt);
    
    /// <summary>
    /// Sets a system prompt for a specific user
    /// </summary>
    void SetUserSystemPrompt(string userId, string systemPrompt);
    
    /// <summary>
    /// Removes custom system prompt for a channel type
    /// </summary>
    void RemoveChannelSystemPrompt(string channelType);
    
    /// <summary>
    /// Removes custom system prompt for a user
    /// </summary>
    void RemoveUserSystemPrompt(string userId);
    
    /// <summary>
    /// Gets all configured system prompts
    /// </summary>
    SystemPromptConfiguration GetConfiguration();
}

public class SystemPromptConfiguration
{
    public string DefaultSystemPrompt { get; set; } = DefaultPrompts.System;
    public Dictionary<string, string> ChannelPrompts { get; set; } = new();
    public Dictionary<string, string> UserPrompts { get; set; } = new();
} 