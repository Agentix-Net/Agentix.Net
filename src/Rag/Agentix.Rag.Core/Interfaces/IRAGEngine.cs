using Agentix.Rag.Core.Models;

namespace Agentix.Rag.Core.Interfaces;

/// <summary>
/// Main interface for RAG (Retrieval Augmented Generation) operations.
/// Provides simple search and indexing capabilities for document sources.
/// </summary>
public interface IRAGEngine
{
    /// <summary>
    /// Searches indexed documents using natural language query.
    /// </summary>
    /// <param name="query">Natural language search query</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Search results with relevant documents</returns>
    Task<RAGResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Indexes all configured document sources (repositories).
    /// This runs automatically on startup but can be called manually to refresh.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task representing the indexing operation</returns>
    Task IndexDocumentsAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets the current status of all configured document sources.
    /// </summary>
    /// <returns>Array of source status information</returns>
    Task<SourceStatus[]> GetSourceStatusAsync();
    
    /// <summary>
    /// Gets whether the RAG engine is ready to handle searches.
    /// Returns true when at least some documents have been indexed.
    /// </summary>
    /// <returns>True if ready for searches, false if still indexing</returns>
    Task<bool> IsReadyAsync();
} 