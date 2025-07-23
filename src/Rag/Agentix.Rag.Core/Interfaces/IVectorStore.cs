using Agentix.Rag.Core.Models;

namespace Agentix.Rag.Core.Interfaces;

/// <summary>
/// Interface for vector storage and similarity search operations.
/// Handles storing document embeddings and finding similar content.
/// </summary>
public interface IVectorStore
{
    /// <summary>
    /// Stores document embeddings in the vector store.
    /// </summary>
    /// <param name="embeddings">Document embeddings to store</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the storage operation</returns>
    Task StoreEmbeddingsAsync(IEnumerable<DocumentEmbedding> embeddings, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Searches for documents similar to the query embedding.
    /// </summary>
    /// <param name="queryEmbedding">Embedding vector for the search query</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="similarityThreshold">Minimum similarity score (0-1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results ordered by similarity</returns>
    Task<IEnumerable<VectorSearchResult>> SearchSimilarAsync(
        float[] queryEmbedding, 
        int maxResults = 10, 
        double similarityThreshold = 0.7, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes all embeddings for documents from a specific source.
    /// </summary>
    /// <param name="sourceId">Source identifier to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the deletion operation</returns>
    Task DeleteBySourceAsync(string sourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the total number of documents stored in the vector store.
    /// </summary>
    /// <returns>Total document count</returns>
    Task<int> GetDocumentCountAsync();
    
    /// <summary>
    /// Clears all stored embeddings.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the clear operation</returns>
    Task ClearAsync(CancellationToken cancellationToken = default);
} 