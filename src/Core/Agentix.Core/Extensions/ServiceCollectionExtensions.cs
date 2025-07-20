using Agentix.Core.Interfaces;
using Agentix.Core.Services;
using Agentix.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Extensions;

/// <summary>
/// Simplified service collection extensions for Agentix
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Agentix core services with simplified configuration
    /// </summary>
    public static AgentixBuilder AddAgentix(this IServiceCollection services, Action<AgentixOptions>? configure = null)
    {
        // Configure options
        var options = new AgentixOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        
        // Register core services
        services.AddSingleton<IAgentixOrchestrator, AgentixOrchestrator>();
        
        // Add logging if not already configured
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });
        
        return new AgentixBuilder(services);
    }
}

/// <summary>
/// Fluent builder for configuring Agentix services
/// </summary>
public class AgentixBuilder
{
    private readonly IServiceCollection _services;

    internal AgentixBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Add an AI provider to the Agentix configuration
    /// </summary>
    public AgentixBuilder AddProvider<T>() where T : class, IAIProvider
    {
        _services.AddSingleton<IAIProvider, T>();
        return this;
    }

    /// <summary>
    /// Add an AI provider instance to the Agentix configuration
    /// </summary>
    public AgentixBuilder AddProvider<T>(T provider) where T : class, IAIProvider
    {
        _services.AddSingleton<IAIProvider>(provider);
        return this;
    }

    /// <summary>
    /// Add a channel adapter to the Agentix configuration
    /// </summary>
    public AgentixBuilder AddChannel<T>() where T : class, IChannelAdapter
    {
        _services.AddSingleton<IChannelAdapter, T>();
        return this;
    }

    /// <summary>
    /// Add a channel adapter instance to the Agentix configuration
    /// </summary>
    public AgentixBuilder AddChannel<T>(T channel) where T : class, IChannelAdapter
    {
        _services.AddSingleton<IChannelAdapter>(channel);
        return this;
    }

    /// <summary>
    /// Access the underlying service collection for advanced configuration
    /// </summary>
    public IServiceCollection Services => _services;
}

/// <summary>
/// Configuration options for Agentix
/// </summary>
public class AgentixOptions
{
    /// <summary>
    /// System prompt to use for AI interactions
    /// </summary>
    public string SystemPrompt { get; set; } = DefaultPrompts.System;

    /// <summary>
    /// Whether to enable cost tracking for AI requests
    /// </summary>
    public bool EnableCostTracking { get; set; } = true;

    /// <summary>
    /// Maximum number of concurrent requests to process
    /// </summary>
    public int MaxConcurrentRequests { get; set; } = 10;
} 