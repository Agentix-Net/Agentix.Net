using Agentix.Core.Interfaces;
using Agentix.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAgentixCore(this IServiceCollection services)
    {
        // Register core services
        services.AddSingleton<IProviderRegistry, ProviderRegistry>();
        services.AddSingleton<IChannelRegistry, ChannelRegistry>();
        services.AddSingleton<ISystemPromptProvider, SystemPromptProvider>();
        services.AddSingleton<IAgentixOrchestrator, AgentixOrchestrator>();
        
        // Add logging if not already configured
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
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
    
    public static IServiceProvider ConfigureSystemPrompts(this IServiceProvider serviceProvider, Action<ISystemPromptProvider> configure)
    {
        var systemPromptProvider = serviceProvider.GetRequiredService<ISystemPromptProvider>();
        configure(systemPromptProvider);
        return serviceProvider;
    }
} 