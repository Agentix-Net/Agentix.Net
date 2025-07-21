using System.Text.Json.Nodes;

namespace Agentix.Core.Models;

/// <summary>
/// Represents an incoming message from a user through any communication channel.
/// Contains all the metadata needed to process and route the message appropriately.
/// </summary>
public sealed class IncomingMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for this message.
    /// </summary>
    /// <value>A unique string identifier for the message. Defaults to a new GUID.</value>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    /// <value>The text content of the user's message.</value>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the unique identifier of the user who sent the message.
    /// </summary>
    /// <value>A string identifying the user across the system.</value>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name of the user who sent the message.
    /// </summary>
    /// <value>The user's display name or username.</value>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the identifier of the specific channel where the message was sent.
    /// </summary>
    /// <value>The channel-specific identifier (e.g., Slack channel ID, Teams conversation ID).</value>
    public string ChannelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name of the channel where the message was sent.
    /// </summary>
    /// <value>The human-readable channel name.</value>
    public string ChannelName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of channel where the message was sent.
    /// </summary>
    /// <value>The channel type (e.g., "console", "slack", "teams").</value>
    public string Channel { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the message was received.
    /// </summary>
    /// <value>The UTC timestamp of when the message was received. Defaults to current UTC time.</value>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the type of the message content.
    /// </summary>
    /// <value>The message type indicating the content format. Defaults to <see cref="MessageType.Text"/>.</value>
    public MessageType Type { get; set; } = MessageType.Text;
    
    /// <summary>
    /// Gets or sets additional metadata associated with the message.
    /// </summary>
    /// <value>A dictionary containing channel-specific or custom metadata.</value>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents a request to an AI provider for generating a response.
/// Contains the message content, configuration, and context information needed for AI processing.
/// </summary>
public sealed class AIRequest
{
    /// <summary>
    /// Gets or sets the content to send to the AI provider.
    /// </summary>
    /// <value>The text content for the AI to process and respond to.</value>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the system prompt to provide context and instructions to the AI.
    /// </summary>
    /// <value>The system prompt that guides the AI's behavior. Defaults to <see cref="DefaultPrompts.System"/>.</value>
    public string SystemPrompt { get; set; } = DefaultPrompts.System;
    
    /// <summary>
    /// Gets or sets the identifier of the user making the request.
    /// </summary>
    /// <value>The user identifier for tracking and personalization.</value>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the identifier of the channel where the request originated.
    /// </summary>
    /// <value>The channel identifier for context and routing.</value>
    public string ChannelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the conversation context identifier.
    /// </summary>
    /// <value>The context identifier linking this request to a conversation history.</value>
    public string ContextId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the original incoming message that triggered this AI request.
    /// </summary>
    /// <value>The original message with full metadata and routing information.</value>
    public IncomingMessage? OriginalMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the AI-specific options and parameters for this request.
    /// </summary>
    /// <value>Configuration options that control AI behavior and output.</value>
    public AIRequestOptions Options { get; set; } = new();
}

/// <summary>
/// Represents the response from an AI provider after processing a request.
/// Contains the generated content, metadata, usage statistics, and error information.
/// </summary>
public sealed class AIResponse
{
    /// <summary>
    /// Gets or sets the generated content from the AI provider.
    /// </summary>
    /// <value>The AI-generated response content.</value>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the AI request was successful.
    /// </summary>
    /// <value>True if the request was processed successfully; otherwise, false.</value>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the error message if the request failed.
    /// </summary>
    /// <value>A description of what went wrong, or null if the request was successful.</value>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the type of response content.
    /// </summary>
    /// <value>The response type indicating the content format. Defaults to <see cref="ResponseType.Text"/>.</value>
    public ResponseType Type { get; set; } = ResponseType.Text;
    
    /// <summary>
    /// Gets or sets the usage metrics for this AI request.
    /// </summary>
    /// <value>Token usage and other consumption metrics.</value>
    public UsageMetrics Usage { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the estimated cost of processing this request.
    /// </summary>
    /// <value>The estimated cost in USD for this AI request.</value>
    public decimal EstimatedCost { get; set; }
    
    /// <summary>
    /// Gets or sets the identifier of the AI provider that processed this request.
    /// </summary>
    /// <value>The name or identifier of the AI provider used.</value>
    public string ProviderId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the specific AI model that was used to generate the response.
    /// </summary>
    /// <value>The model identifier or name used by the provider.</value>
    public string ModelUsed { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the time taken to generate this response.
    /// </summary>
    /// <value>The duration from request start to response completion.</value>
    public TimeSpan ResponseTime { get; set; }
}

/// <summary>
/// Configuration options for AI requests that control generation behavior.
/// </summary>
public sealed class AIRequestOptions
{
    /// <summary>
    /// Gets or sets the creativity/randomness of the AI response.
    /// </summary>
    /// <value>A value between 0.0 (deterministic) and 1.0 (creative). Defaults to 0.7.</value>
    public float Temperature { get; set; } = 0.7f;
    
    /// <summary>
    /// Gets or sets the maximum number of tokens to generate in the response.
    /// </summary>
    /// <value>The maximum output length in tokens. Defaults to 1000.</value>
    public int MaxTokens { get; set; } = 1000;
    
    /// <summary>
    /// Gets or sets the specific AI model to use for this request.
    /// </summary>
    /// <value>The model identifier, or null to use the provider's default model.</value>
    public string? Model { get; set; }
    
    /// <summary>
    /// Gets or sets a system prompt override for this specific request.
    /// </summary>
    /// <value>A system prompt to use instead of the default, or null to use the default.</value>
    public string? SystemPromptOverride { get; set; }
}

/// <summary>
/// Represents token usage metrics for an AI request.
/// Used for cost calculation and usage tracking.
/// </summary>
public sealed class UsageMetrics
{
    /// <summary>
    /// Gets or sets the number of tokens consumed in the input (prompt and context).
    /// </summary>
    /// <value>The input token count.</value>
    public int InputTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the number of tokens generated in the output (response).
    /// </summary>
    /// <value>The output token count.</value>
    public int OutputTokens { get; set; }
    
    /// <summary>
    /// Gets the total number of tokens used for this request.
    /// </summary>
    /// <value>The sum of input and output tokens.</value>
    public int TotalTokens => InputTokens + OutputTokens;
}

/// <summary>
/// Describes the capabilities and features supported by an AI provider.
/// Used for feature detection and routing decisions.
/// </summary>
public sealed class AICapabilities
{
    /// <summary>
    /// Gets or sets a value indicating whether the provider supports function calling/tool use.
    /// </summary>
    /// <value>True if function calling is supported; otherwise, false.</value>
    public bool SupportsFunctionCalling { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the provider supports streaming responses.
    /// </summary>
    /// <value>True if streaming is supported; otherwise, false.</value>
    public bool SupportsStreaming { get; set; }
    
    /// <summary>
    /// Gets or sets a value indicating whether the provider supports vision/image analysis.
    /// </summary>
    /// <value>True if vision capabilities are supported; otherwise, false.</value>
    public bool SupportsVision { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum number of tokens the provider can generate in a single response.
    /// </summary>
    /// <value>The maximum output token limit for this provider.</value>
    public int MaxTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the context window size (total tokens) supported by the provider.
    /// </summary>
    /// <value>The maximum total tokens (input + output) that can be processed.</value>
    public int ContextWindow { get; set; }
    
    /// <summary>
    /// Gets or sets the cost per input token in USD.
    /// </summary>
    /// <value>The cost per input token for billing calculations.</value>
    public decimal CostPerInputToken { get; set; }
    
    /// <summary>
    /// Gets or sets the cost per output token in USD.
    /// </summary>
    /// <value>The cost per output token for billing calculations.</value>
    public decimal CostPerOutputToken { get; set; }
    
    /// <summary>
    /// Gets or sets the models supported by this provider.
    /// </summary>
    /// <value>An enumerable of model identifiers or names available from this provider.</value>
    public IEnumerable<string> SupportedModels { get; set; } = new List<string>();
}

/// <summary>
/// Represents the response from a channel adapter after processing a message.
/// Contains success status and the AI response along with channel-specific metadata.
/// </summary>
public sealed class ChannelResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the channel successfully processed the message.
    /// </summary>
    /// <value>True if processing was successful; otherwise, false.</value>
    public bool Success { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the error message if processing failed.
    /// </summary>
    /// <value>A description of what went wrong, or null if processing was successful.</value>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the AI response generated for the message.
    /// </summary>
    /// <value>The AI response, or null if no response was generated.</value>
    public AIResponse? AIResponse { get; set; }
    
    /// <summary>
    /// Gets or sets additional metadata from the channel processing.
    /// </summary>
    /// <value>A dictionary containing channel-specific metadata and information.</value>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Provides context information for routing and handling messages within channels.
/// </summary>
public sealed class MessageContext
{
    /// <summary>
    /// Gets or sets the identifier of the channel where the message should be sent.
    /// </summary>
    /// <value>The channel identifier for routing the response.</value>
    public string ChannelId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the identifier of the user who should receive the response.
    /// </summary>
    /// <value>The user identifier for targeting the response.</value>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the type of channel for routing decisions.
    /// </summary>
    /// <value>The channel type (e.g., "slack", "console", "teams").</value>
    public string Channel { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the original incoming message that triggered this context.
    /// </summary>
    /// <value>The original message with full metadata.</value>
    public IncomingMessage OriginalMessage { get; set; } = null!;
    
    /// <summary>
    /// Gets or sets additional contextual data for message processing.
    /// </summary>
    /// <value>A dictionary containing additional context information.</value>
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Specifies the type of content in a message.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Plain text message content.
    /// </summary>
    Text,
    
    /// <summary>
    /// File attachment or document.
    /// </summary>
    File,
    
    /// <summary>
    /// Image content.
    /// </summary>
    Image,
    
    /// <summary>
    /// Command or action request.
    /// </summary>
    Command
}

/// <summary>
/// Specifies the type of content in an AI response.
/// </summary>
public enum ResponseType
{
    /// <summary>
    /// Plain text response content.
    /// </summary>
    Text,
    
    /// <summary>
    /// Error response indicating a problem occurred.
    /// </summary>
    Error,
    
    /// <summary>
    /// Result from a tool or function call.
    /// </summary>
    ToolResult,
    
    /// <summary>
    /// Status update or informational message.
    /// </summary>
    StatusUpdate
}

/// <summary>
/// Represents a message within a conversation context.
/// Used for maintaining conversation history and providing context to AI providers.
/// </summary>
public sealed class ContextMessage
{
    /// <summary>
    /// Gets or sets the unique identifier for this context message.
    /// </summary>
    /// <value>A unique string identifier for the message. Defaults to a new GUID.</value>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Gets or sets the role of the message sender.
    /// </summary>
    /// <value>The role identifier (e.g., "user", "assistant", "system", "tool").</value>
    public string Role { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the content of the message.
    /// </summary>
    /// <value>The text content of the message.</value>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the timestamp when the message was created.
    /// </summary>
    /// <value>The UTC timestamp of message creation. Defaults to current UTC time.</value>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the name of the tool that generated this message, if applicable.
    /// </summary>
    /// <value>The tool name if this message came from a tool execution, or null otherwise.</value>
    public string? ToolName { get; set; }
    
    /// <summary>
    /// Gets or sets additional metadata associated with this message.
    /// </summary>
    /// <value>A dictionary containing message-specific metadata and context information.</value>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the number of input tokens consumed for this message.
    /// </summary>
    /// <value>The input token count, or null if not tracked.</value>
    public int? InputTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the number of output tokens generated for this message.
    /// </summary>
    /// <value>The output token count, or null if not tracked.</value>
    public int? OutputTokens { get; set; }
    
    /// <summary>
    /// Gets or sets the cost associated with this message.
    /// </summary>
    /// <value>The cost in USD for processing this message, or null if not tracked.</value>
    public decimal? Cost { get; set; }
}

/// <summary>
/// Represents a key-value data entry stored in conversation context.
/// Provides metadata and expiration information for context data management.
/// </summary>
public sealed class ContextData
{
    /// <summary>
    /// Gets or sets the key identifier for this data entry.
    /// </summary>
    /// <value>The unique key within the conversation context.</value>
    public string Key { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the value stored under this key.
    /// </summary>
    /// <value>The data value, which can be of any type.</value>
    public object? Value { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when this data was stored.
    /// </summary>
    /// <value>The UTC timestamp when the data was set. Defaults to current UTC time.</value>
    public DateTime SetAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the expiration time for this data entry.
    /// </summary>
    /// <value>The UTC timestamp when this data expires, or null if it doesn't expire.</value>
    public DateTime? ExpiresAt { get; set; }
    
    /// <summary>
    /// Gets or sets the type information for deserialization.
    /// </summary>
    /// <value>The type name used for proper deserialization of the value.</value>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents the result of a tool execution within a conversation.
/// Contains execution details, data, and status information for tool calls.
/// </summary>
public sealed class ToolResult
{
    /// <summary>
    /// Gets or sets the unique identifier for the tool call that generated this result.
    /// </summary>
    /// <value>The tool call identifier linking this result to the original request.</value>
    public string ToolCallId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the name of the tool that was executed.
    /// </summary>
    /// <value>The tool identifier or name.</value>
    public string ToolName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets a value indicating whether the tool execution was successful.
    /// </summary>
    /// <value>True if the tool executed successfully; otherwise, false.</value>
    public bool Success { get; set; }
    
    /// <summary>
    /// Gets or sets the data returned by the tool execution.
    /// </summary>
    /// <value>The result data from the tool, which can be of any type.</value>
    public object? Data { get; set; }
    
    /// <summary>
    /// Gets or sets a message describing the tool execution result.
    /// </summary>
    /// <value>A descriptive message about the execution result, or null if not provided.</value>
    public string? Message { get; set; }
    
    /// <summary>
    /// Gets or sets the error message if the tool execution failed.
    /// </summary>
    /// <value>A description of what went wrong, or null if execution was successful.</value>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the time taken to execute the tool.
    /// </summary>
    /// <value>The duration from tool invocation to completion.</value>
    public TimeSpan ExecutionTime { get; set; }
    
    /// <summary>
    /// Gets or sets the timestamp when the tool was executed.
    /// </summary>
    /// <value>The UTC timestamp of tool execution. Defaults to current UTC time.</value>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Gets or sets the operation identifier for long-running tool executions.
    /// </summary>
    /// <value>An identifier for tracking long-running operations, or null for immediate results.</value>
    public string? OperationId { get; set; }
    
    /// <summary>
    /// Gets or sets the current status of the tool execution.
    /// </summary>
    /// <value>The execution status. Defaults to <see cref="ToolExecutionStatus.Completed"/>.</value>
    public ToolExecutionStatus Status { get; set; } = ToolExecutionStatus.Completed;
}

/// <summary>
/// Specifies the execution status of a tool operation.
/// </summary>
public enum ToolExecutionStatus
{
    /// <summary>
    /// The tool execution is waiting to start.
    /// </summary>
    Pending,
    
    /// <summary>
    /// The tool is currently executing.
    /// </summary>
    Running,
    
    /// <summary>
    /// The tool execution completed successfully.
    /// </summary>
    Completed,
    
    /// <summary>
    /// The tool execution failed with an error.
    /// </summary>
    Failed,
    
    /// <summary>
    /// The tool execution was cancelled before completion.
    /// </summary>
    Cancelled
}

/// <summary>
/// Provides default system prompts and templates for AI interactions.
/// </summary>
public static class DefaultPrompts
{
    /// <summary>
    /// The default system prompt used when no custom prompt is specified.
    /// </summary>
    public const string System = "You are a helpful AI assistant. Be concise and accurate in your responses.";
} 