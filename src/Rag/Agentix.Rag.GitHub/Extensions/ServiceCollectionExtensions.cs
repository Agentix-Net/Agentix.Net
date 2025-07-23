using Agentix.Core.Extensions;
using Agentix.Rag.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.GitHub.Extensions;

/// <summary>
/// Extension methods for registering GitHub RAG services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds GitHub RAG functionality to the service collection.
    /// This registers the GitHub document source and embedding provider.
    /// You must also register a vector store implementation (e.g., AddInMemoryVectorStore()).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Configuration action for GitHub RAG options</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGitHubRAG(this IServiceCollection services, Action<GitHubRAGOptions> configure)
    {
        // Configure options
        var options = new GitHubRAGOptions();
        configure(options);
        
        // Validate required configuration
        ValidateOptions(options);
        
        services.AddSingleton(options);
        
        // Register core RAG services
        services.AddSingleton<IDocumentSource, GitHubDocumentSource>();
        
        // Auto-detect and register embedding provider if not already registered
        AutoRegisterEmbeddingProvider(services);
        
        // Register the main RAG engine
        services.AddSingleton<IRAGEngine, GitHubRAGEngine>();
        
        // Register as hosted service to start background indexing
        services.AddHostedService<GitHubRAGEngine>(provider => 
            (GitHubRAGEngine)provider.GetRequiredService<IRAGEngine>());
        
        // Register GitHub search tool
        services.AddSingleton<ITool, GitHubSearchTool>();
        
        return services;
    }
    
    /// <summary>
    /// Adds GitHub RAG functionality with simple repository URLs.
    /// Uses environment variables for API keys.
    /// You must also register a vector store implementation (e.g., AddInMemoryVectorStore()).
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="githubToken">GitHub access token</param>
    /// <param name="repositories">Array of repository URLs</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddGitHubRAG(this IServiceCollection services, string githubToken, params string[] repositories)
    {
        return services.AddGitHubRAG(options =>
        {
            options.AccessToken = githubToken;
            options.Repositories = repositories;
        });
    }
    
    /// <summary>
    /// Adds GitHub RAG functionality to the Agentix builder.
    /// This registers the GitHub document source and embedding provider.
    /// You must also register a vector store implementation (e.g., AddInMemoryVectorStore()).
    /// </summary>
    /// <param name="builder">The Agentix builder</param>
    /// <param name="configure">Configuration action for GitHub RAG options</param>
    /// <returns>The Agentix builder for chaining</returns>
    public static AgentixBuilder AddGitHubRAG(this AgentixBuilder builder, Action<GitHubRAGOptions> configure)
    {
        builder.Services.AddGitHubRAG(configure);
        return builder;
    }
    
    /// <summary>
    /// Adds GitHub RAG functionality to the Agentix builder with simple repository URLs.
    /// Uses environment variables for API keys.
    /// You must also register a vector store implementation (e.g., AddInMemoryVectorStore()).
    /// </summary>
    /// <param name="builder">The Agentix builder</param>
    /// <param name="githubToken">GitHub access token</param>
    /// <param name="repositories">Array of repository URLs</param>
    /// <returns>The Agentix builder for chaining</returns>
    public static AgentixBuilder AddGitHubRAG(this AgentixBuilder builder, string githubToken, params string[] repositories)
    {
        builder.Services.AddGitHubRAG(githubToken, repositories);
        return builder;
    }
    
    private static void ValidateOptions(GitHubRAGOptions options)
    {
        if (options.Repositories.Length == 0)
        {
            throw new ArgumentException("At least one repository URL must be provided");
        }
        
        foreach (var repo in options.Repositories)
        {
            if (string.IsNullOrWhiteSpace(repo))
            {
                throw new ArgumentException("Repository URL cannot be null or empty");
            }
            
            if (!repo.StartsWith("https://github.com/", StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException($"Invalid GitHub URL format: {repo}. Must start with https://github.com/");
            }
        }
    }
    
    private static void AutoRegisterEmbeddingProvider(IServiceCollection services)
    {
        // Check if an embedding provider is already registered
        var existingProvider = services.FirstOrDefault(s => s.ServiceType == typeof(IEmbeddingProvider));
        if (existingProvider != null)
        {
            // Embedding provider already registered, nothing to do
            return;
        }
        
        // Try to detect AI provider and use matching embedding provider
        var aiProviderType = DetectAIProvider(services);
        
        switch (aiProviderType)
        {
            case "Claude":
                // Future: Auto-register Claude embeddings when available
                // For now, fall back to OpenAI
                TryRegisterOpenAIEmbeddings(services);
                break;
            case "OpenAI":
                // Future: Auto-register OpenAI embeddings
                TryRegisterOpenAIEmbeddings(services);
                break;
            default:
                // Default to OpenAI embeddings as fallback
                TryRegisterOpenAIEmbeddings(services);
                break;
        }
    }
    
    private static string DetectAIProvider(IServiceCollection services)
    {
        // Check for registered AI providers by looking at service types
        foreach (var service in services)
        {
            var typeName = service.ServiceType?.Name ?? string.Empty;
            
            if (typeName.Contains("Claude", StringComparison.OrdinalIgnoreCase))
            {
                return "Claude";
            }
            if (typeName.Contains("OpenAI", StringComparison.OrdinalIgnoreCase))
            {
                return "OpenAI";
            }
        }
        
        return "Unknown";
    }
    
    private static void TryRegisterOpenAIEmbeddings(IServiceCollection services)
    {
        try
        {
            // Try to register OpenAI embeddings using reflection to avoid hard dependency
            var assembly = System.Reflection.Assembly.LoadFrom("Agentix.Rag.Embeddings.OpenAI.dll");
            var extensionsType = assembly.GetType("Agentix.Rag.Embeddings.OpenAI.Extensions.ServiceCollectionExtensions");
            var method = extensionsType?.GetMethod("AddOpenAIEmbeddings", new[] { typeof(IServiceCollection) });
            
            if (method != null)
            {
                method.Invoke(null, new object[] { services });
                return;
            }
        }
        catch
        {
            // If reflection fails, fall back to environment check
        }
        
        // Check if OpenAI API key is available
        var openAIKey = GetOpenAIApiKey();
        if (string.IsNullOrEmpty(openAIKey))
        {
            throw new InvalidOperationException(
                "No embedding provider registered and no OpenAI API key found. " +
                "Either:\n" +
                "1. Register an embedding provider explicitly (e.g., services.AddOpenAIEmbeddings())\n" +
                "2. Set the OPENAI_API_KEY environment variable for auto-detection\n" +
                "3. Install Agentix.Rag.Embeddings.OpenAI package");
        }
        
        throw new InvalidOperationException(
            "No embedding provider registered. Please add an embedding provider:\n" +
            "- services.AddOpenAIEmbeddings() (requires Agentix.Rag.Embeddings.OpenAI package)\n" +
            "- Or implement your own IEmbeddingProvider");
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