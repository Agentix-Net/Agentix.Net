using System.Diagnostics;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Core.Helpers;
using Agentix.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

/// <summary>
/// Simplified orchestrator that processes messages using AI providers directly from DI
/// </summary>
public class AgentixOrchestrator : IAgentixOrchestrator
{
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly AgentixOptions _options;
    private readonly ILogger<AgentixOrchestrator> _logger;

    public AgentixOrchestrator(
        IEnumerable<IAIProvider> providers,
        AgentixOptions options,
        ILogger<AgentixOrchestrator> logger)
    {
        _providers = providers;
        _options = options;
        _logger = logger;
    }

    public async Task<AIResponse> ProcessMessageAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        return await ProcessMessageAsync(message, null, cancellationToken);
    }

    public async Task<AIResponse> ProcessMessageAsync(IncomingMessage message, string? providerName = null, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Get provider directly from DI container
            var provider = string.IsNullOrEmpty(providerName) 
                ? _providers.FirstOrDefault()
                : _providers.FirstOrDefault(p => p.Name == providerName);

            if (provider == null)
            {
                var errorMessage = string.IsNullOrEmpty(providerName) 
                    ? "No AI providers are available" 
                    : $"AI provider '{providerName}' not found";
                
                _logger.LogError("Provider not found. Available providers: {AvailableProviders}", 
                               string.Join(", ", _providers.Select(p => p.Name)));
                
                return new AIResponse
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    Type = ResponseType.Error,
                    ResponseTime = stopwatch.Elapsed
                };
            }

            // Build AI request
            var aiRequest = new AIRequest
            {
                Content = message.Content,
                SystemPrompt = _options.SystemPrompt,
                UserId = message.UserId,
                ChannelId = message.ChannelId,
                ContextId = ContextHelpers.GenerateContextId(message),
                OriginalMessage = message
            };

            _logger.LogInformation("Processing message from {UserId} in {Channel} using provider {Provider}", 
                                 message.UserId, message.Channel, provider.Name);

            // Generate response
            var response = await provider.GenerateAsync(aiRequest, cancellationToken);
            response.ResponseTime = stopwatch.Elapsed;

            _logger.LogInformation("Generated response in {Duration}ms using {Provider} (Tokens: {InputTokens}/{OutputTokens}, Cost: ${Cost:F4})", 
                                 stopwatch.ElapsedMilliseconds, provider.Name, 
                                 response.Usage.InputTokens, response.Usage.OutputTokens, response.EstimatedCost);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing message from {UserId}", message.UserId);
            
            return new AIResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while processing your message",
                Type = ResponseType.Error,
                ResponseTime = stopwatch.Elapsed
            };
        }
    }
} 