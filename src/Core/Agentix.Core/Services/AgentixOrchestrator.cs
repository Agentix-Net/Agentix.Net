using System.Diagnostics;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Core.Helpers;
using Agentix.Core.Extensions;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

/// <summary>
/// Orchestrator that processes messages with optional context support using AI providers
/// </summary>
public sealed class AgentixOrchestrator : IAgentixOrchestrator
{
    private readonly IEnumerable<IAIProvider> _providers;
    private readonly IContextStore? _contextStore;
    private readonly IContextResolver? _contextResolver;
    private readonly AgentixOptions _options;
    private readonly ILogger<AgentixOrchestrator> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentixOrchestrator"/> class.
    /// </summary>
    /// <param name="providers">The collection of available AI providers.</param>
    /// <param name="options">The configuration options for the orchestrator.</param>
    /// <param name="logger">The logger for diagnostic and error information.</param>
    /// <param name="contextStore">Optional context store for managing conversation state. If provided, contextResolver must also be provided.</param>
    /// <param name="contextResolver">Optional context resolver for determining conversation boundaries. If provided, contextStore must also be provided.</param>
    /// <exception cref="InvalidOperationException">Thrown when only one of contextStore or contextResolver is provided.</exception>
    public AgentixOrchestrator(
        IEnumerable<IAIProvider> providers,
        AgentixOptions options,
        ILogger<AgentixOrchestrator> logger,
        IContextStore? contextStore = null,
        IContextResolver? contextResolver = null)
    {
        // Validation: both context services must be present together or both null
        if ((contextStore == null) != (contextResolver == null))
        {
            throw new InvalidOperationException(
                "IContextStore and IContextResolver must both be provided or both be null. " +
                "For stateless operation, omit both. For stateful operation, provide both via .AddInMemoryContext() or similar.");
        }

        _providers = providers;
        _contextStore = contextStore;
        _contextResolver = contextResolver;
        _options = options;
        _logger = logger;
    }

    /// <summary>
    /// Processes an incoming message using the first available AI provider.
    /// </summary>
    /// <param name="message">The incoming message to process.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AI response.</returns>
    /// <remarks>
    /// This method uses the first available AI provider from the registered providers.
    /// For more control over provider selection, use the overload that accepts a provider name.
    /// </remarks>
    public async Task<AIResponse> ProcessMessageAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        return await ProcessMessageAsync(message, null, cancellationToken);
    }

    /// <summary>
    /// Processes an incoming message using a specific AI provider or the first available provider.
    /// </summary>
    /// <param name="message">The incoming message to process.</param>
    /// <param name="providerName">The name of the specific AI provider to use, or null to use the first available provider.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AI response.</returns>
    /// <remarks>
    /// This method handles conversation context management (if context services are available), message history, 
    /// and coordinates between the context store and AI provider to generate contextually aware responses.
    /// In stateless mode (no context services), processes messages without conversation history.
    /// </remarks>
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

            // Process with or without context based on availability
            if (_contextStore != null && _contextResolver != null)
            {
                return await ProcessWithContextAsync(message, provider, stopwatch, cancellationToken);
            }
            else
            {
                return await ProcessStatelessAsync(message, provider, stopwatch, cancellationToken);
            }
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

    /// <summary>
    /// Processes a message with context support (stateful mode)
    /// </summary>
    private async Task<AIResponse> ProcessWithContextAsync(IncomingMessage message, IAIProvider provider, Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        // Resolve context for this message
        var contextId = _contextResolver!.ResolveContextId(message);
        
        // Check if we should create a new context
        if (_contextResolver.ShouldCreateNewContext(message))
        {
            await _contextStore!.DeleteContextAsync(contextId);
            _logger.LogInformation("Creating new context for user request: {ContextId}", contextId);
        }
        
        // Get or create context
        var context = await _contextStore!.GetContextAsync(contextId) 
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

    /// <summary>
    /// Processes a message without context support (stateless mode)
    /// </summary>
    private async Task<AIResponse> ProcessStatelessAsync(IncomingMessage message, IAIProvider provider, Stopwatch stopwatch, CancellationToken cancellationToken)
    {
        // Build AI request without context
        var aiRequest = new AIRequest
        {
            Content = message.Content,
            SystemPrompt = _options.SystemPrompt,
            UserId = message.UserId,
            ChannelId = message.ChannelId,
            ContextId = string.Empty, // No context in stateless mode
            OriginalMessage = message
        };

        _logger.LogInformation("Processing message from {UserId} in {Channel} using provider {Provider} (Stateless mode)", 
                             message.UserId, message.Channel, provider.Name);

        // Generate response without context
        var response = await provider.GenerateAsync(aiRequest, cancellationToken);
        response.ResponseTime = stopwatch.Elapsed;

        _logger.LogInformation("Generated response in {Duration}ms using {Provider} (Tokens: {InputTokens}/{OutputTokens}, Cost: ${Cost:F4}) [Stateless]", 
                             stopwatch.ElapsedMilliseconds, provider.Name, 
                             response.Usage.InputTokens, response.Usage.OutputTokens, response.EstimatedCost);

        return response;
    }
} 