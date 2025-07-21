using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Channels.Console.Extensions;

/// <summary>
/// Extension methods for adding Console channel to Agentix
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Console channel adapter to the Agentix configuration
    /// </summary>
    /// <param name="builder">The Agentix builder</param>
    /// <param name="configure">Optional configuration action for ConsoleChannelOptions</param>
    /// <returns>The Agentix builder for chaining</returns>
    public static AgentixBuilder AddConsoleChannel(this AgentixBuilder builder, Action<ConsoleChannelOptions>? configure = null)
    {
        var options = new ConsoleChannelOptions();
        configure?.Invoke(options);
        
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IChannelAdapter, ConsoleChannelAdapter>();
        
        return builder;
    }

    /// <summary>
    /// Adds Console channel adapter to the service collection (for direct DI usage)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration action for ConsoleChannelOptions</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddConsoleChannel(this IServiceCollection services, Action<ConsoleChannelOptions>? configure = null)
    {
        var options = new ConsoleChannelOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IChannelAdapter, ConsoleChannelAdapter>();
        
        return services;
    }
} 