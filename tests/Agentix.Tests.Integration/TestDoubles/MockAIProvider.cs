using Agentix.Core.Interfaces;
using Agentix.Core.Models;

namespace Agentix.Tests.Integration.TestDoubles;

/// <summary>
/// Mock AI provider for testing that doesn't make external API calls
/// </summary>
public class MockAIProvider : IAIProvider
{
    public string Name => "mock-ai";
    
    public AICapabilities Capabilities { get; } = new()
    {
        SupportsStreaming = false,
        SupportsVision = false,
        SupportsFunctionCalling = false,
        MaxTokens = 1000,
        SupportedModels = new[] { "mock-model-1" }
    };

    public bool ShouldFail { get; set; } = false;
    public string MockResponse { get; set; } = "Mock AI response";
    public int CallCount { get; private set; } = 0;

    public Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default)
    {
        CallCount++;
        
        if (ShouldFail)
        {
            return Task.FromResult(new AIResponse
            {
                Success = false,
                ErrorMessage = "Mock provider failure",
                Content = string.Empty,
                Usage = new UsageMetrics(),
                EstimatedCost = 0m
            });
        }

        return Task.FromResult(new AIResponse
        {
            Success = true,
            Content = MockResponse,
            Usage = new UsageMetrics
            {
                InputTokens = request.Content.Length / 4, // Rough estimate
                OutputTokens = MockResponse.Length / 4
            },
            EstimatedCost = 0.001m
        });
    }

    public Task<AIResponse> GenerateWithContextAsync(AIRequest request, IConversationContext context, CancellationToken cancellationToken = default)
    {
        // For testing, just call the regular generate method
        return GenerateAsync(request, cancellationToken);
    }

    public Task<decimal> EstimateCostAsync(AIRequest request)
    {
        if (ShouldFail)
            throw new InvalidOperationException("Mock provider failure");
            
        return Task.FromResult(0.001m);
    }

    public Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(!ShouldFail);
    }
} 