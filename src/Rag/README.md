# Agentix RAG (Retrieval Augmented Generation)

Build AI applications that can search and understand your knowledge sources with natural language.

## 🚀 Quick Start

```bash
dotnet add package Agentix.Rag.GitHub
dotnet add package Agentix.Rag.Embeddings.OpenAI  # Embedding provider
dotnet add package Agentix.Rag.InMemory           # Vector store
```

```csharp
services.AddGitHubRAG(options => {
    options.AccessToken = "ghp_your-github-token";
    options.Repositories = [
        "https://github.com/myorg/repo1",
        "https://github.com/myorg/repo2"
    ];
})
.AddOpenAIEmbeddings()      // Embedding provider
.AddInMemoryVectorStore();  // Vector store implementation
// GitHub search tool automatically included
```

## 📦 Architecture

The Agentix RAG system uses a modular architecture:

### Core Components

- **`Agentix.Rag.Core`** - Core interfaces and models
  - `IRAGEngine` - Main RAG operations
  - `IDocumentSource` - Loading documents from knowledge sources  
  - `IVectorStore` - Storing and searching embeddings
  - `IEmbeddingProvider` - Converting text to vectors
  - `ITool` - RAG search tools interface

### Document Sources

Document sources load content from various knowledge repositories:

- **`Agentix.Rag.GitHub`** ✅ - GitHub repositories
  - Smart file type detection
  - Code, documentation, issues, PRs
  - Multi-repository support
  - Automatic background sync

**Coming Soon:**
- `Agentix.Rag.FileSystem` 🚧 - Local file systems
- `Agentix.Rag.Web` 🚧 - Web scraping
- `Agentix.Rag.SharePoint` 🚧 - SharePoint integration
- `Agentix.Rag.Confluence` 🚧 - Confluence wikis

### Vector Stores

Vector stores handle embedding storage and similarity search:

- **`Agentix.Rag.InMemory`** ✅ - In-memory storage
  - Perfect for development and testing
  - Fast, no external dependencies
  - Supports thousands of documents

**Coming Soon:**
- `Agentix.Rag.Redis` 🚧 - Redis vector search
- `Agentix.Rag.Pinecone` 🚧 - Pinecone integration
- `Agentix.Rag.Weaviate` 🚧 - Weaviate integration

### Embedding Providers

Embedding providers convert text to vectors for semantic search:

- **`Agentix.Rag.Embeddings.OpenAI`** ✅ - OpenAI text embeddings
  - Uses text-embedding-3-small model
  - Cost-effective and high-quality
  - Supports batch processing
  - Works with any AI provider

**Coming Soon:**
- `Agentix.Rag.Embeddings.Azure` 🚧 - Azure OpenAI embeddings
- `Agentix.Rag.Embeddings.Claude` 🚧 - Claude embeddings (when available)
- `Agentix.Rag.Embeddings.Local` 🚧 - Local/self-hosted models

### Tools

RAG tools provide AI models with search capabilities. Tools are co-located with their data sources for better organization:

- **GitHub Search Tool** ✅ - Included with `Agentix.Rag.GitHub`
  - `github_search` - Search GitHub repositories
  - Natural language queries
  - Contextual results
  - Automatically registered when you add GitHub RAG

## 🎯 Usage Patterns

### Basic Setup
```csharp
// Document source + Embedding provider + Vector store (tools included automatically)
services.AddGitHubRAG(options => { ... })
        .AddOpenAIEmbeddings()
        .AddInMemoryVectorStore();
```

### Smart Auto-Detection
```csharp
// When using OpenAI provider, OpenAI embeddings are auto-detected
services.AddOpenAIProvider(options => { ... })     // Future: AI provider
        .AddGitHubRAG(options => { ... })           // Document source + tools
        .AddInMemoryVectorStore();                   // Vector store
// OpenAI embeddings automatically used!
```

### Mixed Providers
```csharp
// Claude for chat, OpenAI for embeddings (explicit override)
services.AddClaudeProvider(options => { ... })     // Claude AI
        .AddGitHubRAG(options => { ... })           // Document source + tools
        .AddOpenAIEmbeddings()                       // Override auto-detection
        .AddRedisVectorStore();                      // Production vector store
```

### Advanced Multi-Source
```csharp
// Multiple knowledge sources with shared embeddings and vector store
services.AddGitHubRAG(options => { ... })           // GitHub repositories + tools
        .AddFileSystemRAG(options => { ... })        // Local files + tools
        .AddWebRAG(options => { ... })               // Web scraping + tools
        .AddOpenAIEmbeddings()                        // Shared embedding provider
        .AddRedisVectorStore();                       // Shared vector store
```

### Custom Vector Store
```csharp
// Implement your own vector store
services.AddSingleton<IVectorStore, MyCustomVectorStore>();
```

## 🏗️ How It Works

1. **Indexing** (Background)
   - Document sources load content
   - Text is chunked intelligently
   - Embeddings are generated
   - Vectors stored in vector store

2. **Search** (Runtime)
   - AI model asks questions
   - RAG tools convert to search queries
   - Vector store finds similar content
   - Results returned to AI model

3. **AI Integration**
   - AI models use search results as context
   - Generate informed responses
   - Maintain conversation memory

## 📚 Learn More

- **[Core Interfaces](Agentix.Rag.Core/README.md)** - Framework interfaces
- **[GitHub Integration](Agentix.Rag.GitHub/README.md)** - Repository search and tools
- **[OpenAI Embeddings](Agentix.Rag.Embeddings.OpenAI/README.md)** - OpenAI text embeddings
- **[Vector Store](Agentix.Rag.InMemory/README.md)** - In-memory storage

## 🚀 Why This Architecture?

✅ **Choose your stack** - Mix document sources and vector stores  
✅ **Start simple** - Begin with in-memory, scale to production stores  
✅ **No vendor lock-in** - Switch vector stores without code changes  
✅ **Logical grouping** - Tools are co-located with their data sources  
✅ **Self-contained** - Each knowledge domain includes everything you need  
✅ **Extensible** - Add custom sources, stores, and tools easily  

## 📄 License

MIT License - build awesome AI apps! 🚀 