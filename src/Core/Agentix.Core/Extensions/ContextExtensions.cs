using Agentix.Core.Interfaces;
using Agentix.Core.Services.Context;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Core.Extensions;

/// <summary>
/// Extension methods for registering context services
/// </summary>
public static class ContextExtensions
{
    /// <summary>
    /// Adds the default context resolver to the service collection
    /// </summary>
    public static IServiceCollection AddContextResolver(this IServiceCollection services)
    {
        services.AddSingleton<IContextResolver, DefaultContextResolver>();
        return services;
    }

    /// <summary>
    /// Adds a custom context resolver to the service collection
    /// </summary>
    public static IServiceCollection AddContextResolver<T>(this IServiceCollection services) 
        where T : class, IContextResolver
    {
        services.AddSingleton<IContextResolver, T>();
        return services;
    }

} 