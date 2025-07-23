using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Embeddings.OpenAI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.Embeddings.OpenAI.Extensions;

/// <summary>
/// Extension methods for registering OpenAI embedding services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OpenAI embedding provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="apiKey">Optional OpenAI API key. If not provided, will try to get from environment variables.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenAIEmbeddings(this IServiceCollection services, string? apiKey = null)
    {
        // Try to get API key from parameter or environment
        var openAIApiKey = apiKey ?? GetOpenAIApiKey();
        
        if (string.IsNullOrEmpty(openAIApiKey))
        {
            throw new InvalidOperationException(
                "OpenAI API key is required for embeddings. " +
                "Provide it as a parameter or set the OPENAI_API_KEY environment variable.");
        }

        // Register the embedding provider
        services.AddSingleton<IEmbeddingProvider>(provider =>
        {
            var logger = provider.GetRequiredService<ILogger<OpenAIEmbeddingProvider>>();
            var httpClient = provider.GetRequiredService<HttpClient>();
            
            return new OpenAIEmbeddingProvider(openAIApiKey, httpClient, logger);
        });
        
        // Register HTTP client for OpenAI API
        services.AddHttpClient<OpenAIEmbeddingProvider>();
        
        return services;
    }
    
    /// <summary>
    /// Adds OpenAI embedding provider with configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configure">Configuration action for OpenAI embedding options.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOpenAIEmbeddings(this IServiceCollection services, Action<OpenAIEmbeddingOptions> configure)
    {
        var options = new OpenAIEmbeddingOptions();
        configure(options);
        
        return services.AddOpenAIEmbeddings(options.ApiKey);
    }
    
    private static string GetOpenAIApiKey()
    {
        // Try various environment variable names
        return Environment.GetEnvironmentVariable("OPENAI_API_KEY") ??
               Environment.GetEnvironmentVariable("OPENAI_KEY") ??
               Environment.GetEnvironmentVariable("OpenAI__ApiKey") ??
               string.Empty;
    }
}

/// <summary>
/// Configuration options for OpenAI embeddings.
/// </summary>
public class OpenAIEmbeddingOptions
{
    /// <summary>
    /// Gets or sets the OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
} 