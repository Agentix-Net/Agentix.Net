using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Providers.Claude.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Providers.Claude;

public class ClaudeProvider : IAIProvider
{
    private readonly HttpClient _httpClient;
    private readonly ClaudeOptions _options;
    private readonly ILogger<ClaudeProvider> _logger;

    public string Name => "claude";

    public AICapabilities Capabilities { get; private set; } = new();

    public ClaudeProvider(ClaudeOptions options, ILogger<ClaudeProvider> logger)
        : this(new HttpClient(), options, logger)
    {
    }

    public ClaudeProvider(HttpClient httpClient, ClaudeOptions options, ILogger<ClaudeProvider> logger)
    {
        _httpClient = httpClient;
        _options = options;
        _logger = logger;

        ConfigureHttpClient();
        InitializeCapabilities();
    }

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

    public void Dispose()
    {
        _httpClient?.Dispose();
    }
} 