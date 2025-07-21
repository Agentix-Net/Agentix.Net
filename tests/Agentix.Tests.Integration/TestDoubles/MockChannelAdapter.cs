using Agentix.Core.Interfaces;
using Agentix.Core.Models;

namespace Agentix.Tests.Integration.TestDoubles;

/// <summary>
/// Mock channel adapter for testing that doesn't interact with external services
/// </summary>
public class MockChannelAdapter : IChannelAdapter
{
    public string Name => "mock-channel";
    public string ChannelType => "mock";
    public bool IsRunning { get; private set; } = false;
    public bool SupportsRichContent => true;
    public bool SupportsFileUploads => false;
    public bool SupportsInteractiveElements => false;

    public bool ShouldFailStart { get; set; } = false;
    public bool ShouldFailProcessing { get; set; } = false;
    public int StartCallCount { get; private set; } = 0;
    public int StopCallCount { get; private set; } = 0;
    public int ProcessCallCount { get; private set; } = 0;
    public List<AIResponse> SentResponses { get; } = new();

    public Task StartAsync(CancellationToken cancellationToken = default)
    {
        StartCallCount++;
        
        if (ShouldFailStart)
            throw new InvalidOperationException("Mock channel start failure");
            
        IsRunning = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken = default)
    {
        StopCallCount++;
        IsRunning = false;
        return Task.CompletedTask;
    }

    public Task<bool> CanHandleAsync(IncomingMessage message)
    {
        return Task.FromResult(message.Channel == ChannelType || message.Channel == "mock");
    }

    public Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        ProcessCallCount++;
        
        if (ShouldFailProcessing)
        {
            return Task.FromResult(new ChannelResponse
            {
                Success = false,
                ErrorMessage = "Mock channel processing failure"
            });
        }

        // Simulate successful processing
        return Task.FromResult(new ChannelResponse
        {
            Success = true,
            AIResponse = new AIResponse
            {
                Success = true,
                Content = "Mock response to: " + message.Content,
                Usage = new UsageMetrics { InputTokens = 10, OutputTokens = 15 },
                EstimatedCost = 0.001m
            }
        });
    }

    public Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default)
    {
        SentResponses.Add(response);
        return Task.CompletedTask;
    }
} 