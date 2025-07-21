using System.Text.Json.Nodes;

namespace Agentix.Core.Models;

public class IncomingMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Content { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ChannelName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty; // "console", "slack", "teams", etc.
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public MessageType Type { get; set; } = MessageType.Text;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class AIRequest
{
    public string Content { get; set; } = string.Empty;
    public string SystemPrompt { get; set; } = DefaultPrompts.System;
    public string UserId { get; set; } = string.Empty;
    public string ChannelId { get; set; } = string.Empty;
    public string ContextId { get; set; } = string.Empty;
    public IncomingMessage? OriginalMessage { get; set; }
    public AIRequestOptions Options { get; set; } = new();
}

public class AIResponse
{
    public string Content { get; set; } = string.Empty;
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public ResponseType Type { get; set; } = ResponseType.Text;
    
    // Usage and cost tracking
    public UsageMetrics Usage { get; set; } = new();
    public decimal EstimatedCost { get; set; }
    
    // Provider information
    public string ProviderId { get; set; } = string.Empty;
    public string ModelUsed { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
}

public class AIRequestOptions
{
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 1000;
    public string? Model { get; set; }
    public string? SystemPromptOverride { get; set; }
}

public class UsageMetrics
{
    public int InputTokens { get; set; }
    public int OutputTokens { get; set; }
    public int TotalTokens => InputTokens + OutputTokens;
}

public class AICapabilities
{
    public bool SupportsFunctionCalling { get; set; }
    public bool SupportsStreaming { get; set; }
    public bool SupportsVision { get; set; }
    public int MaxTokens { get; set; }
    public int ContextWindow { get; set; }
    public decimal CostPerInputToken { get; set; }
    public decimal CostPerOutputToken { get; set; }
    public IEnumerable<string> SupportedModels { get; set; } = new List<string>();
}

public class ChannelResponse
{
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    public AIResponse? AIResponse { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class MessageContext
{
    public string ChannelId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public IncomingMessage OriginalMessage { get; set; } = null!;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

public enum MessageType
{
    Text,
    File,
    Image,
    Command
}

public enum ResponseType
{
    Text,
    Error,
    ToolResult,
    StatusUpdate
}

public static class DefaultPrompts
{
    public const string System = "You are a helpful AI assistant. Be concise and accurate in your responses.";
} 