using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Providers.Claude.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Providers.Claude;

/// <summary>
/// Anthropic Claude AI provider implementation for the Agentix framework.
/// Provides access to Claude's advanced language models including Haiku, Sonnet, and Opus.
/// </summary>
/// <remarks>
/// This provider supports all Claude 3 models and handles cost tracking, error handling,
/// and conversation context management. It automatically manages API communication
/// and token usage calculation.
/// </remarks>
public sealed class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeProvider> _logger;

    /// <summary>
    /// Gets the unique name identifier for this AI provider.
    /// </summary>
    /// <value>Always returns "claude".</value>
    public string Name => "claude";

    /// <summary>
    /// Gets the capabilities supported by this Claude provider instance.
    /// </summary>
    /// <value>An <see cref="AICapabilities"/> object describing Claude's features and limitations.</value>
    public AICapabilities Capabilities { get; private set; } = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaudeProvider"/> class with default HTTP client.
    /// </summary>
    /// <param name="options">Configuration options for the Claude provider.</param>
    /// <param name="logger">Logger instance for diagnostic and error information.</param>
    /// <exception cref="ArgumentException">Thrown when the API key in options is null or empty.</exception>
    public ClaudeProvider(ClaudeOptions options, ILogger<ClaudeProvider> logger)
        : this(new HttpClient(), options, logger)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ClaudeProvider"/> class with a custom HTTP client.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API communications.</param>
    /// <param name="options">Configuration options for the Claude provider.</param>
    /// <param name="logger">Logger instance for diagnostic and error information.</param>
    /// <exception cref="ArgumentException">Thrown when the API key in options is null or empty.</exception>
    /// <remarks>
    /// Using a custom HTTP client allows for advanced scenarios like connection pooling,
    /// custom timeout configurations, or HTTP message handlers for testing.
    /// </remarks>
    public ClaudeProvider(HttpClient httpClient, ClaudeOptions options, ILogger<ClaudeProvider> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        // Validate API key
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            throw new ArgumentException("Claude API key is required and cannot be null or empty", nameof(options));
        }

        ConfigureHttpClient();
        InitializeCapabilities();
        
        _logger.LogInformation("Claude provider initialized with model {Model}", _options.DefaultModel);
    }

    /// <summary>
    /// Generates an AI response for the given request without conversation context.
    /// </summary>
    /// <param name="request">The AI request containing the message and configuration.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AI response from Claude.</returns>
    /// <remarks>
    /// This method sends a single message to Claude without any conversation history.
    /// For conversations that require context, use <see cref="GenerateWithContextAsync"/> instead.
    /// </remarks>
    public async Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var claudeRequest = BuildClaudeRequest(request);
            var response = await CallClaudeApiAsync(claudeRequest, cancellationToken);

            var content = response.Content.FirstOrDefault()?.Text ?? string.Empty;

            return new AIResponse
            {
                Content = content,
                Success = true,
                ProviderId = Name,
                ModelUsed = response.Model,
                Usage = new UsageMetrics
                {
                    InputTokens = response.Usage.InputTokens,
                    OutputTokens = response.Usage.OutputTokens
                },
                EstimatedCost = CalculateCost(response.Usage),
                ResponseTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response with Claude");
            
            return new AIResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                Type = ResponseType.Error,
                ProviderId = Name,
                ResponseTime = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Generates an AI response for the given request with conversation context.
    /// </summary>
    /// <param name="request">The AI request containing the message and configuration.</param>
    /// <param name="context">The conversation context containing message history and data.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AI response from Claude.</returns>
    /// <remarks>
    /// This method includes conversation history from the context when generating the response,
    /// allowing Claude to maintain coherent conversations and refer to previous messages.
    /// The number of historical messages included is controlled by <see cref="ClaudeOptions.MaxHistoryMessages"/>.
    /// </remarks>
    public async Task<AIResponse> GenerateWithContextAsync(AIRequest request, IConversationContext context, CancellationToken cancellationToken = default)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            var claudeRequest = await BuildClaudeRequestWithContextAsync(request, context);
            var response = await CallClaudeApiAsync(claudeRequest, cancellationToken);

            var content = response.Content.FirstOrDefault()?.Text ?? string.Empty;

            return new AIResponse
            {
                Content = content,
                Success = true,
                ProviderId = Name,
                ModelUsed = response.Model,
                Usage = new UsageMetrics
                {
                    InputTokens = response.Usage.InputTokens,
                    OutputTokens = response.Usage.OutputTokens
                },
                EstimatedCost = CalculateCost(response.Usage),
                ResponseTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating response with Claude using context");
            
            return new AIResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                Type = ResponseType.Error,
                ProviderId = Name,
                ResponseTime = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Estimates the cost for processing the given AI request.
    /// </summary>
    /// <param name="request">The AI request to estimate cost for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the estimated cost in USD.</returns>
    /// <remarks>
    /// This method provides a rough cost estimation based on character count approximation.
    /// Actual costs may vary slightly due to differences in tokenization. The estimation
    /// uses the current model's pricing as configured in <see cref="Capabilities"/>.
    /// </remarks>
    public Task<decimal> EstimateCostAsync(AIRequest request)
    {
        // Rough estimation based on character count
        // Claude's actual tokenization would be more accurate
        var estimatedInputTokens = request.Content.Length / 4; // Rough approximation
        var estimatedOutputTokens = _options.MaxTokens;

        var cost = (estimatedInputTokens * Capabilities.CostPerInputToken) + 
                  (estimatedOutputTokens * Capabilities.CostPerOutputToken);
        
        return Task.FromResult(cost);
    }

    /// <summary>
    /// Performs a health check to verify the Claude provider is working correctly.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the provider is healthy, false otherwise.</returns>
    /// <remarks>
    /// The health check sends a simple "Hello" message to Claude with minimal token usage
    /// to verify API connectivity, authentication, and basic functionality.
    /// </remarks>
    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var healthRequest = new AIRequest
            {
                Content = "Hello",
                Options = new AIRequestOptions { MaxTokens = 10 }
            };

            var response = await GenerateAsync(healthRequest, cancellationToken);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Claude health check failed");
            return false;
        }
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
        _httpClient.DefaultRequestHeaders.Add("anthropic-version", "2023-06-01");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);
    }

    private void InitializeCapabilities()
    {
        Capabilities = new AICapabilities
        {
            SupportsFunctionCalling = false, // Claude doesn't support function calling yet
            SupportsStreaming = false,       // Not implemented in this basic version
            SupportsVision = _options.DefaultModel.Contains("claude-3"), // Claude 3 supports vision
            MaxTokens = GetMaxTokensForModel(_options.DefaultModel),
            ContextWindow = GetContextWindowForModel(_options.DefaultModel),
            CostPerInputToken = GetInputCostForModel(_options.DefaultModel),
            CostPerOutputToken = GetOutputCostForModel(_options.DefaultModel),
            SupportedModels = new[]
            {
                "claude-3-opus-20240229",
                "claude-3-sonnet-20240229", 
                "claude-3-haiku-20240307"
            }
        };
    }

    private ClaudeRequest BuildClaudeRequest(AIRequest request)
    {
        var claudeRequest = new ClaudeRequest
        {
            Model = request.Options.Model ?? _options.DefaultModel,
            MaxTokens = request.Options.MaxTokens > 0 ? request.Options.MaxTokens : _options.MaxTokens,
            Temperature = request.Options.Temperature,
            Messages = new List<ClaudeMessage>
            {
                new ClaudeMessage
                {
                    Role = "user",
                    Content = request.Content
                }
            }
        };

        // Set system prompt if provided and not the default
        var systemPrompt = request.Options.SystemPromptOverride ?? request.SystemPrompt;
        if (!string.IsNullOrEmpty(systemPrompt) && 
            systemPrompt != DefaultPrompts.System && 
            systemPrompt.Trim().Length > 0)
        {
            claudeRequest.System = systemPrompt;
        }

        return claudeRequest;
    }

    private async Task<ClaudeRequest> BuildClaudeRequestWithContextAsync(AIRequest request, IConversationContext context)
    {
        var claudeRequest = new ClaudeRequest
        {
            Model = request.Options.Model ?? _options.DefaultModel,
            MaxTokens = request.Options.MaxTokens > 0 ? request.Options.MaxTokens : _options.MaxTokens,
            Temperature = request.Options.Temperature,
            Messages = new List<ClaudeMessage>()
        };

        // Set system prompt if provided and not the default
        var systemPrompt = request.Options.SystemPromptOverride ?? request.SystemPrompt;
        if (!string.IsNullOrEmpty(systemPrompt) && 
            systemPrompt != DefaultPrompts.System && 
            systemPrompt.Trim().Length > 0)
        {
            claudeRequest.System = systemPrompt;
        }

        // Add conversation history
        var messages = await context.GetMessagesAsync(_options.MaxHistoryMessages);
        foreach (var msg in messages.Where(m => m.Role != "system"))
        {
            // Map context message roles to Claude roles
            var claudeRole = msg.Role switch
            {
                "user" => "user",
                "assistant" => "assistant",
                _ => "user" // Default unknown roles to user
            };

            claudeRequest.Messages.Add(new ClaudeMessage
            {
                Role = claudeRole,
                Content = msg.Content
            });
        }

        // Add the current message (should already be in context, but ensure it's there)
        if (!claudeRequest.Messages.Any() || claudeRequest.Messages.Last().Content != request.Content)
        {
            claudeRequest.Messages.Add(new ClaudeMessage
            {
                Role = "user",
                Content = request.Content
            });
        }

        return claudeRequest;
    }

    private async Task<ClaudeResponse> CallClaudeApiAsync(ClaudeRequest request, CancellationToken cancellationToken)
    {
        var json = JsonSerializer.Serialize(request);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _logger.LogDebug("Calling Claude API with model {Model}", request.Model);

        var response = await _httpClient.PostAsync("/v1/messages", content, cancellationToken);

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Claude API error: {StatusCode} - {Content}", response.StatusCode, responseContent);
            
            // Handle specific error types
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException("Invalid Claude API key. Please check your API key and try again.");
            }
            
            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                throw new UnauthorizedAccessException("Claude API key does not have permission to access this resource.");
            }
            
            try
            {
                var error = JsonSerializer.Deserialize<ClaudeError>(responseContent);
                throw new HttpRequestException($"Claude API error: {error?.Error?.Message ?? "Unknown error"}");
            }
            catch (JsonException)
            {
                throw new HttpRequestException($"Claude API error: {response.StatusCode} - {responseContent}");
            }
        }

        var claudeResponse = JsonSerializer.Deserialize<ClaudeResponse>(responseContent);
        if (claudeResponse == null)
        {
            throw new InvalidOperationException("Failed to deserialize Claude response");
        }

        return claudeResponse;
    }

    private decimal CalculateCost(ClaudeUsage usage)
    {
        return (usage.InputTokens * Capabilities.CostPerInputToken) + 
               (usage.OutputTokens * Capabilities.CostPerOutputToken);
    }

    private int GetMaxTokensForModel(string model)
    {
        return model switch
        {
            "claude-3-opus-20240229" => 4096,
            "claude-3-sonnet-20240229" => 4096,
            "claude-3-haiku-20240307" => 4096,
            _ => 4096
        };
    }

    private int GetContextWindowForModel(string model)
    {
        return model switch
        {
            "claude-3-opus-20240229" => 200000,
            "claude-3-sonnet-20240229" => 200000,
            "claude-3-haiku-20240307" => 200000,
            _ => 200000
        };
    }

    private decimal GetInputCostForModel(string model)
    {
        // Costs per 1K tokens as of 2024 (these may change)
        return model switch
        {
            "claude-3-opus-20240229" => 0.015m,
            "claude-3-sonnet-20240229" => 0.003m,
            "claude-3-haiku-20240307" => 0.00025m,
            _ => 0.003m
        };
    }

    private decimal GetOutputCostForModel(string model)
    {
        // Costs per 1K tokens as of 2024 (these may change)
        return model switch
        {
            "claude-3-opus-20240229" => 0.075m,
            "claude-3-sonnet-20240229" => 0.015m,
            "claude-3-haiku-20240307" => 0.00125m,
            _ => 0.015m
        };
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ClaudeProvider"/>.
    /// </summary>
    /// <remarks>
    /// This method disposes the internal HTTP client if it was created by this provider.
    /// If a custom HTTP client was provided in the constructor, it will also be disposed.
    /// </remarks>
    public void Dispose()
    {
        _httpClient?.Dispose();
    }
} 