using Agentix.Core.Extensions;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.InMemory;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Rag.InMemory.Extensions;

/// <summary>
/// Extension methods for registering in-memory vector store services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the in-memory vector store implementation to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddInMemoryVectorStore(this IServiceCollection services)
    {
        services.AddSingleton<IVectorStore, InMemoryVectorStore>();
        return services;
    }
    
    /// <summary>
    /// Adds the in-memory vector store implementation to the Agentix builder.
    /// </summary>
    /// <param name="builder">The Agentix builder to add services to.</param>
    /// <returns>The Agentix builder for chaining.</returns>
    public static AgentixBuilder AddInMemoryVectorStore(this AgentixBuilder builder)
    {
        builder.Services.AddInMemoryVectorStore();
        return builder;
    }
} 