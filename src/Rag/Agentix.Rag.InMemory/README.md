# Agentix.Rag.InMemory

In-memory vector store implementation for the Agentix RAG Engine.

## Overview

This package provides a simple, fast in-memory vector store that's perfect for:
- Development and testing
- Small-scale deployments
- Prototyping RAG applications
- Scenarios where you don't want external dependencies

## Features

- ✅ Fast in-memory vector storage and search
- ✅ Cosine similarity search with configurable thresholds
- ✅ Concurrent operations support
- ✅ Source-based document management
- ✅ No external dependencies (beyond .NET)

## Installation

```bash
dotnet add package Agentix.Rag.InMemory
```

## Quick Start

```csharp
using Agentix.Rag.InMemory.Extensions;

// Register the in-memory vector store
services.AddInMemoryVectorStore();
```

## Usage with GitHub RAG

```csharp
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.InMemory.Extensions;
using Agentix.Rag.Tools.Extensions;

await Host.CreateDefaultBuilder(args)
    .ConfigureServices(services => services
        .AddAgentixCore()
        .AddClaudeProvider(options => options.ApiKey = "your-claude-api-key")
        .AddConsoleChannel()
        .AddGitHubRAG(options => {
            options.AccessToken = "your-github-token";
            options.Repositories = [
                "https://github.com/your-org/repo1",
                "https://github.com/your-org/repo2"
            ];
        })
        .AddInMemoryVectorStore()  // Vector store implementation
        .AddRAGTools())            // RAG search tools
    .BuildAndRunAgentixAsync();
```

## Architecture

The in-memory vector store:

1. **Storage**: Uses `ConcurrentDictionary` for thread-safe operations
2. **Search**: Implements cosine similarity for vector search
3. **Performance**: Optimized for small to medium-sized document collections
4. **Memory**: All data is stored in RAM (lost on restart)

## Limitations

- **Memory Usage**: All embeddings are stored in RAM
- **Persistence**: Data is lost when the application restarts
- **Scalability**: Suitable for thousands of documents, not millions
- **Clustering**: Not suitable for multi-instance deployments

## Alternative Vector Stores

For production scenarios, consider:
- Redis-based vector store (coming soon)
- Pinecone integration (coming soon)
- Weaviate integration (coming soon)

## Configuration

The in-memory vector store requires no configuration. It uses sensible defaults:

- **Similarity Threshold**: 0.7 (configurable per search)
- **Max Results**: 10 (configurable per search)
- **Concurrency**: Full thread-safety support

## Performance Characteristics

- **Insert**: O(1) per document
- **Search**: O(n) where n = total documents
- **Memory**: ~4KB per 1000-dimension embedding
- **Concurrency**: Read/write operations are thread-safe

## Best Practices

1. **Memory Management**: Monitor memory usage with large document sets
2. **Search Tuning**: Adjust similarity thresholds based on your use case
3. **Periodic Cleanup**: Use `DeleteBySourceAsync()` to remove outdated documents
4. **Development**: Perfect for local development and testing

## API Reference

### IVectorStore Implementation

```csharp
public class InMemoryVectorStore : IVectorStore
{
    Task StoreEmbeddingsAsync(IEnumerable<DocumentEmbedding> embeddings, CancellationToken cancellationToken = default);
    Task<IEnumerable<VectorSearchResult>> SearchSimilarAsync(float[] queryEmbedding, int maxResults = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default);
    Task DeleteBySourceAsync(string sourceId, CancellationToken cancellationToken = default);
    Task<int> GetDocumentCountAsync();
    Task ClearAsync(CancellationToken cancellationToken = default);
}
```

## License

MIT License - build awesome AI apps! 🚀 