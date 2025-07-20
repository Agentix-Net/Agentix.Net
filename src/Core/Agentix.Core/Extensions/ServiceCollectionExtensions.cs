using Agentix.Core.Interfaces;
using Agentix.Core.Services;
using Agentix.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentixCore(this IServiceCollection services, Action<AgentixOptions>? configure = null)
    {
        // Configure options
        var options = new AgentixOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        
        // Register core services
        services.AddSingleton<IProviderRegistry, ProviderRegistry>();
        services.AddSingleton<IChannelRegistry, ChannelRegistry>();
        services.AddSingleton<IAgentixOrchestrator, AgentixOrchestrator>();
        
        // Add logging if not already configured
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });
        
        return services;
    }
    
    public static IServiceCollection AddAgentixProvider<T>(this IServiceCollection services, T provider)
        where T : class, IAIProvider
    {
        services.AddSingleton<IAIProvider>(provider);
        return services;
    }
    
    public static IServiceCollection AddAgentixChannel<T>(this IServiceCollection services, T channel)
        where T : class, IChannelAdapter
    {
        services.AddSingleton<IChannelAdapter>(channel);
        return services;
    }
}

public class AgentixOptions
{
    public string SystemPrompt { get; set; } = DefaultPrompts.System;
    public bool EnableCostTracking { get; set; } = true;
    public int MaxConcurrentRequests { get; set; } = 10;
} 