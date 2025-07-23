using Agentix.Rag.Core.Models;

namespace Agentix.Rag.Core.Interfaces;

/// <summary>
/// Interface for document sources that can provide documents for indexing.
/// Examples: GitHub repositories, file systems, databases, etc.
/// </summary>
public interface IDocumentSource
{
    /// <summary>
    /// Gets the unique name of this document source.
    /// </summary>
    string Name { get; }
    
    /// <summary>
    /// Gets the type of source (e.g., "github", "filesystem", "database").
    /// </summary>
    string SourceType { get; }
    
    /// <summary>
    /// Loads documents from this source for indexing.
    /// </summary>
    /// <param name="sourceConfig">Configuration specific to this source</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Documents ready for indexing</returns>
    Task<IEnumerable<Document>> LoadDocumentsAsync(SourceConfig sourceConfig, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if this source can handle the given configuration.
    /// </summary>
    /// <param name="sourceConfig">Configuration to check</param>
    /// <returns>True if this source can handle the configuration</returns>
    Task<bool> CanHandleAsync(SourceConfig sourceConfig);
    
    /// <summary>
    /// Gets the current status of this source.
    /// </summary>
    /// <param name="sourceConfig">Configuration for the source</param>
    /// <returns>Current status information</returns>
    Task<SourceStatus> GetStatusAsync(SourceConfig sourceConfig);
} 