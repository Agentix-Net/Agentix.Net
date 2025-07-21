using Agentix.Core.Interfaces;
using Agentix.Core.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Context.InMemory.Extensions;

/// <summary>
/// Extension methods for registering in-memory context services
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds in-memory context storage to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration action</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddInMemoryContext(this IServiceCollection services, 
        Action<InMemoryContextOptions>? configure = null)
    {
        var options = new InMemoryContextOptions();
        configure?.Invoke(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IContextStore, InMemoryContextStore>();
        
        return services;
    }

    /// <summary>
    /// Adds in-memory context storage to the Agentix builder
    /// </summary>
    /// <param name="builder">The Agentix builder</param>
    /// <param name="configure">Optional configuration action</param>
    /// <returns>The Agentix builder for chaining</returns>
    public static AgentixBuilder AddInMemoryContext(this AgentixBuilder builder, 
        Action<InMemoryContextOptions>? configure = null)
    {
        builder.Services.AddInMemoryContext(configure);
        return builder;
    }
}

/// <summary>
/// Configuration options for in-memory context storage
/// </summary>
public class InMemoryContextOptions
{
    /// <summary>
    /// How often to run context cleanup
    /// </summary>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Maximum number of messages to keep per context
    /// </summary>
    public int MaxMessagesPerContext { get; set; } = 100;
    
    /// <summary>
    /// Maximum number of tool results to keep per context
    /// </summary>
    public int MaxToolResultsPerContext { get; set; } = 50;
    
    /// <summary>
    /// Default context expiration time
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(4);
} 