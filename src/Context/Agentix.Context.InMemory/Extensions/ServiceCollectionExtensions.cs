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
        
        // Also register the default context resolver since both are needed together
        services.AddContextResolver();
        
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
/// Configuration options for in-memory context storage.
/// Controls cleanup behavior, memory limits, and expiration settings for conversation contexts.
/// </summary>
public sealed class InMemoryContextOptions
{
    /// <summary>
    /// Gets or sets how often to run expired context cleanup.
    /// </summary>
    /// <value>The cleanup interval. Defaults to 5 minutes.</value>
    /// <remarks>
    /// The cleanup process removes expired conversations from memory to prevent memory leaks.
    /// Shorter intervals provide more timely cleanup but use more CPU resources.
    /// Longer intervals use less CPU but may allow expired contexts to accumulate.
    /// </remarks>
    public TimeSpan CleanupInterval { get; set; } = TimeSpan.FromMinutes(5);
    
    /// <summary>
    /// Gets or sets the maximum number of messages to keep per conversation context.
    /// </summary>
    /// <value>The maximum message count per context. Defaults to 100.</value>
    /// <remarks>
    /// When this limit is exceeded, the oldest messages are automatically removed to maintain
    /// memory usage. This affects conversation history available to AI providers.
    /// Higher values provide more context but use more memory.
    /// </remarks>
    public int MaxMessagesPerContext { get; set; } = 100;
    
    /// <summary>
    /// Gets or sets the maximum number of tool results to keep per conversation context.
    /// </summary>
    /// <value>The maximum tool result count per context. Defaults to 50.</value>
    /// <remarks>
    /// When this limit is exceeded, the oldest tool results are automatically removed.
    /// Tool results provide history of function calls and their outcomes within conversations.
    /// Higher values provide more complete tool history but use more memory.
    /// </remarks>
    public int MaxToolResultsPerContext { get; set; } = 50;
    
    /// <summary>
    /// Gets or sets the default expiration time for conversation contexts.
    /// </summary>
    /// <value>The default context expiration duration. Defaults to 4 hours.</value>
    /// <remarks>
    /// Contexts are automatically removed after this duration of inactivity.
    /// Individual contexts can extend their expiration, but this provides the default.
    /// Longer expiration times allow for extended conversations but use more memory.
    /// Shorter times free up memory faster but may interrupt long conversations.
    /// </remarks>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromHours(4);
} 