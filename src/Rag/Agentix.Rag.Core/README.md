# Agentix.Rag.Core

Core interfaces and models for RAG (Retrieval Augmented Generation) functionality in Agentix.Net framework. This package provides the foundational abstractions that all RAG implementations build upon.

## Overview

This package contains:
- **Core Interfaces**: `IRAGEngine`, `IDocumentSource`, `IVectorStore`, `IEmbeddingProvider`
- **Models**: Document types, search results, configuration models
- **Abstractions**: Common types and enums for RAG operations

## Key Interfaces

### IRAGEngine
Main interface for RAG operations:
```csharp
public interface IRAGEngine
{
    Task<RAGResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default);
    Task IndexDocumentsAsync(CancellationToken cancellationToken = default);
    Task<SourceStatus[]> GetSourceStatusAsync();
    Task<bool> IsReadyAsync();
}
```

### IDocumentSource
Interface for loading documents from various sources:
```csharp
public interface IDocumentSource
{
    Task<IEnumerable<Document>> LoadDocumentsAsync(SourceConfig sourceConfig, CancellationToken cancellationToken = default);
    Task<bool> CanHandleAsync(SourceConfig sourceConfig);
    Task<SourceStatus> GetStatusAsync(SourceConfig sourceConfig);
}
```

### IVectorStore
Interface for storing and searching document embeddings:
```csharp
public interface IVectorStore
{
    Task StoreEmbeddingsAsync(IEnumerable<DocumentEmbedding> embeddings, CancellationToken cancellationToken = default);
    Task<IEnumerable<VectorSearchResult>> SearchSimilarAsync(float[] queryEmbedding, int maxResults = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default);
}
```

### IEmbeddingProvider
Interface for generating text embeddings:
```csharp
public interface IEmbeddingProvider
{
    Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default);
    Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default);
}
```

## Core Models

### Document
Represents a document that can be indexed and searched:
```csharp
public class Document
{
    public string Id { get; set; }
    public string Content { get; set; }
    public string Title { get; set; }
    public DocumentType Type { get; set; }
    public string SourceId { get; set; }
    public string SourceName { get; set; }
    public string? Url { get; set; }
    public string? Path { get; set; }
    public DateTime LastModified { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
}
```

### RAGResult
Result from a search query:
```csharp
public class RAGResult
{
    public string Query { get; set; }
    public DocumentResult[] Documents { get; set; }
    public int TotalResults { get; set; }
    public TimeSpan QueryTime { get; set; }
    public DateTime Timestamp { get; set; }
}
```

## Installation

```bash
dotnet add package Agentix.Rag.Core
```

## Usage

This is a core package that provides interfaces and models. You typically don't use it directly, but through implementation packages like:

- `Agentix.Rag.GitHub` - GitHub repository integration
- `Agentix.Rag.Tools` - RAG search tools

## Next Steps

- Install an implementation package like `Agentix.Rag.GitHub`
- Check out the samples for complete examples
- Read the architecture documentation for design details

## Contributing

See [CONTRIBUTING.md](../../../CONTRIBUTING.md) for guidelines on extending the RAG system. 