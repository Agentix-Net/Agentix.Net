namespace Agentix.Rag.Core.Models;

/// <summary>
/// Represents a document that can be indexed and searched.
/// </summary>
public class Document
{
    /// <summary>
    /// Unique identifier for this document.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Main content of the document.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Title or name of the document.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Type of document (Code, Documentation, Issue, etc.).
    /// </summary>
    public DocumentType Type { get; set; }
    
    /// <summary>
    /// Identifier of the source this document came from.
    /// </summary>
    public string SourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable source name (e.g., "myorg/myrepo").
    /// </summary>
    public string SourceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Direct URL to the original document (e.g., GitHub file URL).
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Path within the source (e.g., file path in repository).
    /// </summary>
    public string? Path { get; set; }
    
    /// <summary>
    /// When this document was last modified.
    /// </summary>
    public DateTime LastModified { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Additional metadata about the document.
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Represents the type of document content.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// Source code file.
    /// </summary>
    Code,
    
    /// <summary>
    /// Documentation file (README, docs, etc.).
    /// </summary>
    Documentation,
    
    /// <summary>
    /// Issue or bug report.
    /// </summary>
    Issue,
    
    /// <summary>
    /// Pull request or merge request.
    /// </summary>
    PullRequest,
    
    /// <summary>
    /// Wiki page.
    /// </summary>
    Wiki,
    
    /// <summary>
    /// Configuration file.
    /// </summary>
    Configuration,
    
    /// <summary>
    /// Other/unknown type.
    /// </summary>
    Other
}

/// <summary>
/// Document with its embedding vector for storage in vector database.
/// </summary>
public class DocumentEmbedding
{
    /// <summary>
    /// The document being embedded.
    /// </summary>
    public Document Document { get; set; } = null!;
    
    /// <summary>
    /// The embedding vector for this document.
    /// </summary>
    public float[] Embedding { get; set; } = Array.Empty<float>();
    
    /// <summary>
    /// When this embedding was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Result from a RAG search query.
/// </summary>
public class RAGResult
{
    /// <summary>
    /// The original search query.
    /// </summary>
    public string Query { get; set; } = string.Empty;
    
    /// <summary>
    /// Documents found matching the query.
    /// </summary>
    public DocumentResult[] Documents { get; set; } = Array.Empty<DocumentResult>();
    
    /// <summary>
    /// Total number of results found (may be more than returned).
    /// </summary>
    public int TotalResults { get; set; }
    
    /// <summary>
    /// Time taken to execute the search.
    /// </summary>
    public TimeSpan QueryTime { get; set; }
    
    /// <summary>
    /// When this search was performed.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// A document result from a search query.
/// </summary>
public class DocumentResult
{
    /// <summary>
    /// The document content (may be truncated for display).
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// Title of the document.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Source repository name (e.g., "myorg/myrepo").
    /// </summary>
    public string Repository { get; set; } = string.Empty;
    
    /// <summary>
    /// File path within the repository.
    /// </summary>
    public string FilePath { get; set; } = string.Empty;
    
    /// <summary>
    /// Direct URL to view the document.
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// Similarity score (0-1, higher is more similar).
    /// </summary>
    public double Similarity { get; set; }
    
    /// <summary>
    /// Type of document.
    /// </summary>
    public DocumentType Type { get; set; }
    
    /// <summary>
    /// When the document was last modified.
    /// </summary>
    public DateTime LastModified { get; set; }
}

/// <summary>
/// Result from vector similarity search.
/// </summary>
public class VectorSearchResult
{
    /// <summary>
    /// The document found.
    /// </summary>
    public Document Document { get; set; } = null!;
    
    /// <summary>
    /// Similarity score (0-1, higher is more similar).
    /// </summary>
    public double Similarity { get; set; }
    
    /// <summary>
    /// Distance metric (lower is more similar).
    /// </summary>
    public double Distance { get; set; }
}

/// <summary>
/// Configuration for a document source.
/// </summary>
public class SourceConfig
{
    /// <summary>
    /// Unique identifier for this source.
    /// </summary>
    public string Id { get; set; } = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Type of source (e.g., "github", "filesystem").
    /// </summary>
    public string SourceType { get; set; } = string.Empty;
    
    /// <summary>
    /// Source-specific configuration data.
    /// For GitHub: URL, access token, etc.
    /// </summary>
    public Dictionary<string, object> Configuration { get; set; } = new();
    
    /// <summary>
    /// Human-readable name for this source.
    /// </summary>
    public string? Name { get; set; }
    
    /// <summary>
    /// Whether this source is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Status information for a document source.
/// </summary>
public class SourceStatus
{
    /// <summary>
    /// Source identifier.
    /// </summary>
    public string SourceId { get; set; } = string.Empty;
    
    /// <summary>
    /// Human-readable source name.
    /// </summary>
    public string SourceName { get; set; } = string.Empty;
    
    /// <summary>
    /// Current status of the source.
    /// </summary>
    public IndexingStatus Status { get; set; }
    
    /// <summary>
    /// Total number of documents from this source.
    /// </summary>
    public int DocumentCount { get; set; }
    
    /// <summary>
    /// When this source was last updated.
    /// </summary>
    public DateTime LastUpdated { get; set; }
    
    /// <summary>
    /// Error message if status is Error.
    /// </summary>
    public string? ErrorMessage { get; set; }
    
    /// <summary>
    /// Progress percentage (0-100) if currently indexing.
    /// </summary>
    public int ProgressPercentage { get; set; }
}

/// <summary>
/// Indexing status for a document source.
/// </summary>
public enum IndexingStatus
{
    /// <summary>
    /// Source has not been indexed yet.
    /// </summary>
    NotStarted,
    
    /// <summary>
    /// Currently indexing documents.
    /// </summary>
    Indexing,
    
    /// <summary>
    /// Successfully indexed and ready for search.
    /// </summary>
    Ready,
    
    /// <summary>
    /// Error occurred during indexing.
    /// </summary>
    Error,
    
    /// <summary>
    /// Source is temporarily unavailable.
    /// </summary>
    Unavailable
} 