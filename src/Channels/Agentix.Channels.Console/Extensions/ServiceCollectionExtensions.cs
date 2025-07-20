using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Channels.Console.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConsoleChannel(this IServiceCollection services, Action<ConsoleChannelOptions>? configure = null)
    {
        var options = new ConsoleChannelOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IChannelAdapter, ConsoleChannelAdapter>();
        
        return services;
    }
} 