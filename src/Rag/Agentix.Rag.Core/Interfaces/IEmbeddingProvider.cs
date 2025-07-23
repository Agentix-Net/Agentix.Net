namespace Agentix.Rag.Core.Interfaces;

/// <summary>
/// Interface for generating text embeddings for semantic search.
/// </summary>
public interface IEmbeddingProvider
{
    /// <summary>
    /// Gets the name of this embedding provider.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the dimension size of embeddings produced by this provider.
    /// </summary>
    int EmbeddingDimension { get; }
    
    /// <summary>
    /// Generates an embedding vector for the given text.
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Embedding vector as float array</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates embeddings for multiple texts in batch.
    /// More efficient than calling GenerateEmbeddingAsync multiple times.
    /// </summary>
    /// <param name="texts">Texts to generate embeddings for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Embedding vectors in the same order as input texts</returns>
    Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if the embedding provider is healthy and available.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if provider is available</returns>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
} 