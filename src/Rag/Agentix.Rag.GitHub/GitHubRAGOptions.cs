namespace Agentix.Rag.GitHub;

/// <summary>
/// Configuration options for GitHub RAG integration.
/// Designed for simplicity - just provide access token and repository URLs.
/// </summary>
public class GitHubRAGOptions
{
    /// <summary>
    /// GitHub personal access token.
    /// Required for private repositories, optional for public ones.
    /// Get from: https://github.com/settings/tokens
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;
    
    /// <summary>
    /// Array of GitHub repository URLs to index.
    /// Examples: ["https://github.com/myorg/repo1", "https://github.com/myorg/repo2"]
    /// </summary>
    public string[] Repositories { get; set; } = Array.Empty<string>();
    
    // Internal smart defaults - users don't need to configure these
    internal TimeSpan SyncInterval => TimeSpan.FromHours(6);
    internal int MaxFileSize => 50_000; // 50KB max per file
    internal int ChunkSize => 1000; // Characters per chunk
    internal int ChunkOverlap => 200; // Overlap between chunks
    internal int MaxConcurrentOperations => 3; // Don't overwhelm GitHub API
    
    /// <summary>
    /// Common file extensions to index by default.
    /// Automatically filtered based on repository language detection.
    /// </summary>
    internal string[] DefaultFileExtensions => 
    [
        ".cs", ".js", ".ts", ".py", ".java", ".go", ".rs", ".cpp", ".c", ".h", // Code
        ".md", ".txt", ".rst", ".adoc", // Documentation
        ".json", ".yml", ".yaml", ".xml", ".toml" // Configuration
    ];
    
    /// <summary>
    /// Common directories to exclude from indexing.
    /// </summary>
    internal string[] DefaultExcludePaths => 
    [
        "node_modules/", "bin/", "obj/", ".git/", "dist/", "build/", 
        "target/", ".vs/", ".vscode/", "packages/", "__pycache__/"
    ];
} 