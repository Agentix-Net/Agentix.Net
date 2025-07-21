namespace Agentix.Providers.Claude;

/// <summary>
/// Configuration options for the Claude AI provider.
/// Contains settings for API access, model selection, and behavior customization.
/// </summary>
public sealed class ClaudeOptions
{
    /// <summary>
    /// Gets or sets the Anthropic API key for authenticating with Claude services.
    /// </summary>
    /// <value>The API key obtained from the Anthropic Console. This field is required.</value>
    /// <remarks>
    /// You can obtain an API key from https://console.anthropic.com/.
    /// Keep this key secure and never commit it to source control.
    /// </remarks>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the base URL for the Claude API.
    /// </summary>
    /// <value>The API endpoint URL. Defaults to "https://api.anthropic.com".</value>
    /// <remarks>
    /// Typically you won't need to change this unless you're using a proxy
    /// or a different API environment.
    /// </remarks>
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    
    /// <summary>
    /// Gets or sets the default Claude model to use for requests.
    /// </summary>
    /// <value>The model identifier. Defaults to "claude-3-haiku-20240307".</value>
    /// <remarks>
    /// Available models include:
    /// <list type="bullet">
    /// <item><description>claude-3-haiku-20240307 - Fast and cost-effective</description></item>
    /// <item><description>claude-3-sonnet-20240229 - Balanced performance and cost</description></item>
    /// <item><description>claude-3-opus-20240229 - Highest capability and reasoning</description></item>
    /// </list>
    /// This can be overridden per request using <see cref="Core.Models.AIRequestOptions.Model"/>.
    /// </remarks>
    public string DefaultModel { get; set; } = "claude-3-haiku-20240307";
    
    /// <summary>
    /// Gets or sets the default temperature for response generation.
    /// </summary>
    /// <value>A value between 0.0 and 1.0 controlling randomness. Defaults to 0.7.</value>
    /// <remarks>
    /// Lower values (0.0-0.3) produce more focused and deterministic responses.
    /// Higher values (0.7-1.0) produce more creative and varied responses.
    /// This can be overridden per request using <see cref="Core.Models.AIRequestOptions.Temperature"/>.
    /// </remarks>
    public float Temperature { get; set; } = 0.7f;
    
    /// <summary>
    /// Gets or sets the default maximum number of tokens to generate in responses.
    /// </summary>
    /// <value>The maximum output tokens. Defaults to 1000.</value>
    /// <remarks>
    /// This controls the maximum length of Claude's responses. Higher values allow longer responses
    /// but increase cost and latency. This can be overridden per request using 
    /// <see cref="Core.Models.AIRequestOptions.MaxTokens"/>.
    /// </remarks>
    public int MaxTokens { get; set; } = 1000;
    
    /// <summary>
    /// Gets or sets the timeout in seconds for API requests.
    /// </summary>
    /// <value>The timeout duration in seconds. Defaults to 30 seconds.</value>
    /// <remarks>
    /// Increase this value if you're making requests that require longer processing time,
    /// such as complex reasoning tasks or requests with large context windows.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic retry on transient failures.
    /// </summary>
    /// <value>True to enable retry logic; otherwise, false. Defaults to true.</value>
    /// <remarks>
    /// When enabled, the provider will automatically retry failed requests for transient errors
    /// such as rate limiting or temporary network issues. The number of retries is controlled
    /// by <see cref="MaxRetries"/>.
    /// </remarks>
    public bool EnableRetry { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed requests.
    /// </summary>
    /// <value>The maximum retry count. Defaults to 3.</value>
    /// <remarks>
    /// Only applies when <see cref="EnableRetry"/> is true. The provider uses exponential backoff
    /// between retry attempts to avoid overwhelming the API.
    /// </remarks>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets the maximum number of historical messages to include in context-aware requests.
    /// </summary>
    /// <value>The maximum message count for conversation history. Defaults to 10.</value>
    /// <remarks>
    /// When using <see cref="ClaudeProvider.GenerateWithContextAsync"/>, this determines
    /// how many previous messages from the conversation history are sent to Claude.
    /// Higher values provide more context but increase token usage and cost.
    /// </remarks>
    public int MaxHistoryMessages { get; set; } = 10;
} 