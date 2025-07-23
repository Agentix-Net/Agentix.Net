# Agentix.Rag.GitHub

GitHub repository integration for RAG (Retrieval Augmented Generation) in Agentix.Net framework. This package allows your AI to search and understand code repositories using natural language queries.

## Features

- ✅ **Simple setup** - Just provide GitHub token and repository URLs
- ✅ **Smart indexing** - Automatically detects relevant file types based on repository language
- ✅ **Multi-repository** - Search across multiple repositories simultaneously
- ✅ **Full content** - Indexes code, documentation, issues, and pull requests
- ✅ **Background sync** - Automatically keeps repositories up-to-date
- ✅ **Modular design** - Choose your own vector store implementation

## Installation

```bash
dotnet add package Agentix.Rag.GitHub
dotnet add package Agentix.Rag.Embeddings.OpenAI  # Embedding provider
dotnet add package Agentix.Rag.InMemory           # Vector store
```

## Quick Start

### 1. Get API Keys

You'll need:
- **GitHub Token**: [Generate here](https://github.com/settings/tokens) (needs `repo` scope for private repos)
- **OpenAI API Key**: [Get from OpenAI](https://platform.openai.com/api-keys) (for embeddings)

### 2. Setup Environment Variables

```bash
# Required for embeddings
OPENAI_API_KEY=sk-your-openai-api-key

# Optional - can also pass directly in code
GITHUB_TOKEN=ghp_your-github-token
```

### 3. Add to Your Application

```csharp
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.Embeddings.OpenAI.Extensions;
using Agentix.Rag.InMemory.Extensions;

services.AddGitHubRAG(options => {
    options.AccessToken = "ghp_your-github-token";
    options.Repositories = [
        "https://github.com/myorg/backend-api",
        "https://github.com/myorg/frontend-app",
        "https://github.com/myorg/documentation"
    ];
})
.AddOpenAIEmbeddings()      // Embedding provider
.AddInMemoryVectorStore();  // Vector store implementation
```

### 4. Use in Your Code

```csharp
public class MyService
{
    private readonly IRAGEngine _ragEngine;
    
    public MyService(IRAGEngine ragEngine)
    {
        _ragEngine = ragEngine;
    }
    
    public async Task<string> SearchCodebase(string query)
    {
        var results = await _ragEngine.SearchAsync(query, maxResults: 5);
        
        foreach (var doc in results.Documents)
        {
            Console.WriteLine($"Found in {doc.Repository}: {doc.Title}");
            Console.WriteLine($"Similarity: {doc.Similarity:P}");
            Console.WriteLine($"URL: {doc.Url}");
            Console.WriteLine($"Content: {doc.Content}");
            Console.WriteLine();
        }
        
        return $"Found {results.TotalResults} relevant documents";
    }
}
```

## Configuration Options

### Simple Configuration
```csharp
services.AddGitHubRAG(options => {
    options.AccessToken = "ghp_your-token";
    options.Repositories = [
        "https://github.com/myorg/repo1",
        "https://github.com/myorg/repo2"
    ];
})
.AddOpenAIEmbeddings()      // Required: Add embedding provider
.AddInMemoryVectorStore();  // Required: Add vector store
```

### Environment-based Configuration
```csharp
services.AddGitHubRAG(options => {
    options.AccessToken = configuration["GitHub:Token"];
    options.Repositories = configuration.GetSection("GitHub:Repositories").Get<string[]>();
})
.AddOpenAIEmbeddings()      // Auto-detects OPENAI_API_KEY from environment
.AddInMemoryVectorStore();
```

### Direct Method
```csharp
services.AddGitHubRAG(
    githubToken: "ghp_your-token",
    "https://github.com/myorg/repo1",
    "https://github.com/myorg/repo2"
)
.AddOpenAIEmbeddings()
.AddInMemoryVectorStore();
```

## What Gets Indexed

The system automatically indexes relevant content based on repository analysis:

### Code Files
Automatically detected based on repository's primary language:
- **C#**: `.cs`, `.csproj`, `.sln`
- **TypeScript**: `.ts`, `.tsx`, `.js`, `.jsx`
- **Python**: `.py`, `.yml`, `.yaml`
- **Java**: `.java`, `.xml`
- **Go**: `.go`, `.mod`

### Documentation
Always included:
- `.md`, `.txt`, `.rst`, `.adoc` files
- README files
- Wiki pages (if accessible)

### Issues & Pull Requests
- Recent issues and pull requests
- Provides context about problems and recent changes
- Automatically excluded for documentation-only repositories

### Automatic Exclusions
Common directories are automatically skipped:
- `node_modules/`, `bin/`, `obj/`, `.git/`
- `dist/`, `build/`, `target/`, `.vs/`
- Files larger than 50KB (configurable internally)

## Search Examples

```csharp
// Find authentication implementation
var authResults = await ragEngine.SearchAsync("How is authentication implemented?");

// Find error handling patterns
var errorResults = await ragEngine.SearchAsync("error handling patterns");

// Find specific APIs
var apiResults = await ragEngine.SearchAsync("user management API endpoints");

// Find configuration examples
var configResults = await ragEngine.SearchAsync("database connection configuration");
```

## Built-in GitHub Search Tool

The GitHub RAG package includes a built-in search tool that's automatically registered:

```csharp
services.AddGitHubRAG(options => { /* config */ })
        .AddOpenAIEmbeddings()     // Required for semantic search
        .AddInMemoryVectorStore(); // Required for vector storage
// The github_search tool is automatically available to your AI

// Now your AI can automatically search repositories:
// User: "How do we handle authentication?"
// AI: [Uses github_search tool to find relevant code]
```

## Embedding Provider Flexibility

Choose your embedding provider based on your AI provider or preferences:

```csharp
// Option 1: OpenAI embeddings (most common)
services.AddGitHubRAG(options => { ... })
        .AddOpenAIEmbeddings()
        .AddInMemoryVectorStore();

// Option 2: Auto-detection (future)
services.AddClaudeProvider(options => { ... })      // Claude for chat
        .AddGitHubRAG(options => { ... })            // Auto-detects compatible embeddings
        .AddInMemoryVectorStore();

// Option 3: Explicit override
services.AddClaudeProvider(options => { ... })      // Claude for chat
        .AddGitHubRAG(options => { ... })
        .AddOpenAIEmbeddings()                        // Force OpenAI embeddings
        .AddInMemoryVectorStore();
```

## How It Works

1. **Repository Analysis**: Detects repository language and characteristics
2. **Smart Filtering**: Automatically selects relevant file types
3. **Content Extraction**: Downloads and processes files, issues, PRs
4. **Intelligent Chunking**: Breaks large files into searchable chunks
5. **Embedding Generation**: Creates vector embeddings using OpenAI
6. **Vector Storage**: Stores embeddings in fast in-memory vector store
7. **Semantic Search**: Finds most relevant content using cosine similarity
8. **Background Sync**: Automatically updates content every 6 hours

## Repository Status

Check indexing status:

```csharp
var statuses = await ragEngine.GetSourceStatusAsync();

foreach (var status in statuses)
{
    Console.WriteLine($"{status.SourceName}: {status.Status}");
    if (status.Status == IndexingStatus.Ready)
    {
        Console.WriteLine($"  {status.DocumentCount} documents indexed");
        Console.WriteLine($"  Last updated: {status.LastUpdated}");
    }
}

// Check if ready for searches
var isReady = await ragEngine.IsReadyAsync();
```

## Performance Considerations

- **Indexing Time**: Depends on repository size and API rate limits
- **Memory Usage**: Stores embeddings in memory (~6KB per document chunk)
- **API Costs**: Uses OpenAI embeddings (very cost-effective)
- **Rate Limits**: Respects GitHub and OpenAI API limits automatically

## Troubleshooting

### "Repository not found or access denied"
- Verify GitHub token has access to the repository
- For private repos, ensure token has `repo` scope
- Check repository URL format

### "OpenAI API error"
- Verify `OPENAI_API_KEY` environment variable is set
- Check API key has sufficient credits
- Ensure key format is correct (`sk-...`)

### "No documents loaded"
- Repository might be empty or have no supported file types
- Check if repository has been archived or deleted
- Review logs for specific error messages

### Performance Issues
- Large repositories take longer to index initially
- Consider excluding large binary files or generated code
- Monitor OpenAI API usage and costs

## Example Integration

Complete example with Agentix framework:

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.Tools.Extensions;

services.AddAgentix()
    .AddClaudeProvider(options => options.ApiKey = claudeKey)
    .AddConsoleChannel()
    .AddGitHubRAG(options => {
        options.AccessToken = githubToken;
        options.Repositories = ["https://github.com/myorg/myrepo"];
    })
    .AddRAGTools();

// Now your AI can search your codebase!
```

## See Also

- [`Agentix.Rag.Tools`](../Agentix.Rag.Tools/) - RAG search tools for AI integration
- [`Agentix.Sample.RAG.Console`](../../../samples/Agentix.Sample.RAG.Console/) - Complete working example
- [Core Documentation](../../Core/Agentix.Core/README.md) - Framework basics 