using System.Diagnostics;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

public class AgentixOrchestrator : IAgentixOrchestrator
{
    private readonly IProviderRegistry _providerRegistry;
    private readonly IChannelRegistry _channelRegistry;
    private readonly ISystemPromptProvider _systemPromptProvider;
    private readonly ILogger<AgentixOrchestrator> _logger;
    private bool _isRunning;

    public AgentixOrchestrator(
        IProviderRegistry providerRegistry,
        IChannelRegistry channelRegistry,
        ISystemPromptProvider systemPromptProvider,
        ILogger<AgentixOrchestrator> logger)
    {
        _providerRegistry = providerRegistry;
        _channelRegistry = channelRegistry;
        _systemPromptProvider = systemPromptProvider;
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
            // Get the appropriate provider
            var provider = string.IsNullOrEmpty(providerName) 
                ? _providerRegistry.GetDefaultProvider() 
                : _providerRegistry.GetProvider(providerName);

            if (provider == null)
            {
                var errorMessage = string.IsNullOrEmpty(providerName) 
                    ? "No AI providers are registered" 
                    : $"AI provider '{providerName}' not found";
                
                return new AIResponse
                {
                    Success = false,
                    ErrorMessage = errorMessage,
                    Type = ResponseType.Error
                };
            }

            // Build AI request
            var aiRequest = new AIRequest
            {
                Content = message.Content,
                UserId = message.UserId,
                ChannelId = message.ChannelId,
                ContextId = GenerateContextId(message),
                OriginalMessage = message
            };

            // Get system prompt from the system prompt provider
            var systemPrompt = _systemPromptProvider.GetSystemPrompt(
                channelType: message.Channel, 
                userId: message.UserId, 
                contextId: aiRequest.ContextId);
            
            aiRequest.SystemPrompt = systemPrompt;

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
                ErrorMessage = $"An error occurred while processing your message: {ex.Message}",
                Type = ResponseType.Error,
                ResponseTime = stopwatch.Elapsed
            };
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            _logger.LogWarning("Agentix orchestrator is already running");
            return;
        }

        _logger.LogInformation("Starting Agentix orchestrator...");

        // Start all registered channels
        var channelRegistry = _channelRegistry as ChannelRegistry;
        if (channelRegistry != null)
        {
            await channelRegistry.StartAllChannelsAsync(cancellationToken);
        }

        _isRunning = true;
        _logger.LogInformation("Agentix orchestrator started successfully");
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!_isRunning)
        {
            _logger.LogWarning("Agentix orchestrator is not running");
            return;
        }

        _logger.LogInformation("Stopping Agentix orchestrator...");

        // Stop all channels
        var channelRegistry = _channelRegistry as ChannelRegistry;
        if (channelRegistry != null)
        {
            await channelRegistry.StopAllChannelsAsync(cancellationToken);
        }

        _isRunning = false;
        _logger.LogInformation("Agentix orchestrator stopped successfully");
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            // Check if we have at least one provider
            var providers = _providerRegistry.GetAllProviders();
            if (!providers.Any())
            {
                return false;
            }

            // Check health of default provider
            var defaultProvider = _providerRegistry.GetDefaultProvider();
            if (defaultProvider == null)
            {
                return false;
            }

            return await defaultProvider.HealthCheckAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return false;
        }
    }

    public async Task<SystemStatus> GetStatusAsync()
    {
        var status = new SystemStatus
        {
            RegisteredProviders = _providerRegistry.GetAllProviders().Count(),
            RunningChannels = _channelRegistry.GetRunningChannels().Count()
        };

        // Check provider health
        foreach (var provider in _providerRegistry.GetAllProviders())
        {
            try
            {
                status.ProviderHealth[provider.Name] = await provider.HealthCheckAsync();
            }
            catch
            {
                status.ProviderHealth[provider.Name] = false;
            }
        }

        // Check channel status
        foreach (var channel in _channelRegistry.GetAllChannels())
        {
            status.ChannelStatus[channel.Name] = channel.IsRunning;
        }

        status.IsHealthy = status.ProviderHealth.Values.Any(h => h) && _isRunning;
        
        return status;
    }

    private string GenerateContextId(IncomingMessage message)
    {
        // Simple context ID generation for now - will be enhanced with context management later
        return $"{message.Channel}:{message.ChannelId}:{message.UserId}";
    }
} 