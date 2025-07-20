using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Providers.Claude.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddClaudeProvider(this IServiceCollection services, Action<ClaudeOptions> configure)
    {
        var options = new ClaudeOptions();
        configure(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IAIProvider, ClaudeProvider>();
        
        return services;
    }
} 