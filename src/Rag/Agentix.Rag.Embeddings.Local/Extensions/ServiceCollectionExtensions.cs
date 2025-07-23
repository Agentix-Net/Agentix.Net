using Agentix.Core.Extensions;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Embeddings.Local;
using Agentix.Rag.Embeddings.Local.Models;
using Agentix.Rag.Embeddings.Local.Tokenization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.Embeddings.Local.Extensions;

/// <summary>
/// Extension methods for registering local embedding services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds local embedding provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="cacheDirectory">Optional custom cache directory for models. If not provided, uses default user directory.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalEmbeddings(this IServiceCollection services, string? cacheDirectory = null)
    {
        // Register the embedding provider components
        services.AddSingleton<BertTokenizer>();
        
        services.AddSingleton<ModelManager>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<ModelManager>>();
            var httpClient = provider.GetRequiredService<HttpClient>();
            
            return new ModelManager(logger, httpClient, cacheDirectory);
        });
        
        services.AddSingleton<IEmbeddingProvider>(provider =>
        {
            var modelManager = provider.GetRequiredService<ModelManager>();
            var tokenizer = provider.GetRequiredService<BertTokenizer>();
            var logger = provider.GetRequiredService<ILogger<LocalEmbeddingProvider>>();
            
            return new LocalEmbeddingProvider(modelManager, tokenizer, logger);
        });
        
        // Register HTTP client for model downloads
        services.AddHttpClient<ModelManager>();
        
        return services;
    }
    
    /// <summary>
    /// Adds local embedding provider with configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Configuration action for local embedding options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalEmbeddings(this IServiceCollection services, Action<LocalEmbeddingOptions> configure)
    {
        var options = new LocalEmbeddingOptions();
        configure(options);
        
        return services.AddLocalEmbeddings(options.CacheDirectory);
    }
    
    /// <summary>
    /// Adds local embedding provider with specific model configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="modelName">Name of the model to use (e.g., "all-MiniLM-L6-v2")</param>
    /// <param name="cacheDirectory">Optional custom cache directory for models</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddLocalEmbeddings(this IServiceCollection services, string modelName, string? cacheDirectory = null)
    {
        return services.AddLocalEmbeddings(options =>
        {
            options.ModelName = modelName;
            options.CacheDirectory = cacheDirectory;
        });
    }
    
    /// <summary>
    /// Adds local embedding provider to the Agentix builder.
    /// </summary>
    /// <param name="builder">The Agentix builder to add services to.</param>
    /// <param name="cacheDirectory">Optional custom cache directory for models. If not provided, uses default user directory.</param>
    /// <returns>The Agentix builder for chaining.</returns>
    public static AgentixBuilder AddLocalEmbeddings(this AgentixBuilder builder, string? cacheDirectory = null)
    {
        builder.Services.AddLocalEmbeddings(cacheDirectory);
        return builder;
    }
    
    /// <summary>
    /// Adds local embedding provider to the Agentix builder with configuration.
    /// </summary>
    /// <param name="builder">The Agentix builder to add services to.</param>
    /// <param name="configure">Configuration action for local embedding options.</param>
    /// <returns>The Agentix builder for chaining.</returns>
    public static AgentixBuilder AddLocalEmbeddings(this AgentixBuilder builder, Action<LocalEmbeddingOptions> configure)
    {
        builder.Services.AddLocalEmbeddings(configure);
        return builder;
    }
    
    /// <summary>
    /// Adds local embedding provider to the Agentix builder with specific model configuration.
    /// </summary>
    /// <param name="builder">The Agentix builder to add services to.</param>
    /// <param name="modelName">Name of the model to use (e.g., "all-MiniLM-L6-v2")</param>
    /// <param name="cacheDirectory">Optional custom cache directory for models</param>
    /// <returns>The Agentix builder for chaining.</returns>
    public static AgentixBuilder AddLocalEmbeddings(this AgentixBuilder builder, string modelName, string? cacheDirectory = null)
    {
        builder.Services.AddLocalEmbeddings(modelName, cacheDirectory);
        return builder;
    }
}

/// <summary>
/// Configuration options for local embeddings.
/// </summary>
public class LocalEmbeddingOptions
{
    /// <summary>
    /// Gets or sets the model name to use for embeddings.
    /// </summary>
    public string ModelName { get; set; } = ModelManager.DefaultModelName;
    
    /// <summary>
    /// Gets or sets the cache directory for storing downloaded models.
    /// </summary>
    public string? CacheDirectory { get; set; }
    
    /// <summary>
    /// Gets or sets the maximum sequence length for tokenization.
    /// </summary>
    public int MaxSequenceLength { get; set; } = 512;
    
    /// <summary>
    /// Gets or sets whether to automatically download models on first use.
    /// </summary>
    public bool AutoDownloadModels { get; set; } = true;
} 