using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Providers.Claude.Extensions;

/// <summary>
/// Extension methods for adding Claude provider to Agentix
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Claude AI provider to the Agentix configuration
    /// </summary>
    /// <param name="builder">The Agentix builder</param>
    /// <param name="configure">Configuration action for Claude options</param>
    /// <returns>The Agentix builder for chaining</returns>
    public static AgentixBuilder AddClaudeProvider(this AgentixBuilder builder, Action<ClaudeOptions> configure)
    {
        var options = new ClaudeOptions();
        configure(options);
        
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IAIProvider, ClaudeProvider>();
        
        return builder;
    }

    /// <summary>
    /// Adds Claude AI provider to the service collection (for direct DI usage)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for Claude options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddClaudeProvider(this IServiceCollection services, Action<ClaudeOptions> configure)
    {
        var options = new ClaudeOptions();
        configure(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IAIProvider, ClaudeProvider>();
        
        return services;
    }
} 