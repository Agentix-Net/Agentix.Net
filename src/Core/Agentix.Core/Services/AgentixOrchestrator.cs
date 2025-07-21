using System.Diagnostics;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Core.Helpers;
using Agentix.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

/// <summary>
/// Orchestrator that processes messages with context support using AI providers
/// </summary>
public class AgentixOrchestrator : IAgentixOrchestrator
{
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly IContextStore _contextStore;
    private readonly IContextResolver _contextResolver;
    private readonly AgentixOptions _options;
    private readonly ILogger<AgentixOrchestrator> _logger;

    public AgentixOrchestrator(
        IEnumerable<IAIProvider> providers,
        IContextStore contextStore,
        IContextResolver contextResolver,
        AgentixOptions options,
        ILogger<AgentixOrchestrator> logger)
    {
        _providers = providers;
        _contextStore = contextStore;
        _contextResolver = contextResolver;
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

            // Resolve context for this message
            var contextId = _contextResolver.ResolveContextId(message);
            
            // Check if we should create a new context
            if (_contextResolver.ShouldCreateNewContext(message))
            {
                await _contextStore.DeleteContextAsync(contextId);
                _logger.LogInformation("Creating new context for user request: {ContextId}", contextId);
            }
            
            // Get or create context
            var context = await _contextStore.GetContextAsync(contextId) 
                         ?? await _contextStore.CreateContextAsync(contextId, message.UserId, message.ChannelId, message.Channel);

            // Add user message to context
            await context.AddMessageAsync(new ContextMessage
            {
                Role = "user",
                Content = message.Content,
                Metadata = { 
                    ["channel"] = message.Channel,
                    ["channelId"] = message.ChannelId,
                    ["userName"] = message.UserName
                }
            });

            // Build AI request
            var aiRequest = new AIRequest
            {
                Content = message.Content,
                SystemPrompt = _options.SystemPrompt,
                UserId = message.UserId,
                ChannelId = message.ChannelId,
                ContextId = contextId,
                OriginalMessage = message
            };

            _logger.LogInformation("Processing message from {UserId} in {Channel} using provider {Provider} (Context: {ContextId})", 
                                 message.UserId, message.Channel, provider.Name, contextId);

            // Generate response with context
            AIResponse response;
            if (provider.GetType().GetMethod("GenerateWithContextAsync") != null)
            {
                // Provider supports context
                response = await provider.GenerateWithContextAsync(aiRequest, context, cancellationToken);
            }
            else
            {
                // Fallback to non-context method
                response = await provider.GenerateAsync(aiRequest, cancellationToken);
            }
            
            response.ResponseTime = stopwatch.Elapsed;

            // Save assistant response to context
            if (response.Success)
            {
                await context.AddMessageAsync(new ContextMessage
                {
                    Role = "assistant",
                    Content = response.Content,
                    InputTokens = response.Usage.InputTokens,
                    OutputTokens = response.Usage.OutputTokens,
                    Cost = response.EstimatedCost,
                    Metadata = { 
                        ["model"] = response.ModelUsed,
                        ["provider"] = response.ProviderId
                    }
                });
            }

            // Save context changes
            await _contextStore.SaveContextAsync(context);

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