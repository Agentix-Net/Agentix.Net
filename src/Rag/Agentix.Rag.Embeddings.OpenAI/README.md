# Agentix.Rag.Embeddings.OpenAI

OpenAI embedding provider for the Agentix RAG Engine.

## Overview

This package provides OpenAI text embeddings for Agentix RAG applications. It uses the `text-embedding-3-small` model for cost-effective, high-quality embeddings.

## Features

- ✅ **Cost-effective** - Uses text-embedding-3-small model (lowest cost option)
- ✅ **High performance** - Batch embedding support for efficiency
- ✅ **Production ready** - Error handling, logging, and health checks
- ✅ **Flexible configuration** - Environment variables or explicit API keys
- ✅ **Auto-detection** - Works seamlessly with Agentix smart provider detection

## Installation

```bash
dotnet add package Agentix.Rag.Embeddings.OpenAI
```

## Quick Start

### Option 1: Environment Variable (Recommended)
```bash
# Set your OpenAI API key
export OPENAI_API_KEY=sk_your-openai-api-key
```

```csharp
using Agentix.Rag.Embeddings.OpenAI.Extensions;

// Auto-detects API key from environment
services.AddOpenAIEmbeddings();
```

### Option 2: Explicit Configuration
```csharp
using Agentix.Rag.Embeddings.OpenAI.Extensions;

services.AddOpenAIEmbeddings("sk_your-openai-api-key");

// Or with configuration
services.AddOpenAIEmbeddings(options =>
{
    options.ApiKey = "sk_your-openai-api-key";
});
```

## Integration with RAG

### Explicit Configuration
```csharp
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.Embeddings.OpenAI.Extensions;
using Agentix.Rag.InMemory.Extensions;

services.AddGitHubRAG(options => { ... })
        .AddOpenAIEmbeddings()     // Explicit embedding provider
        .AddInMemoryVectorStore();
```

### Auto-Detection (Smart Default)
```csharp
using Agentix.Providers.OpenAI.Extensions;  // Future
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.InMemory.Extensions;

// When using OpenAI provider, OpenAI embeddings are auto-detected
services.AddOpenAIProvider(options => { ... })
        .AddGitHubRAG(options => { ... })
        .AddInMemoryVectorStore();
// OpenAI embeddings automatically used!
```

### Mixed Providers
```csharp
using Agentix.Providers.Claude.Extensions;
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.Embeddings.OpenAI.Extensions;
using Agentix.Rag.InMemory.Extensions;

// Use Claude for chat, OpenAI for embeddings
services.AddClaudeProvider(options => { ... })
        .AddGitHubRAG(options => { ... })
        .AddOpenAIEmbeddings()     // Override auto-detection
        .AddInMemoryVectorStore();
```

## Embedding Model Details

- **Model**: `text-embedding-3-small`
- **Dimensions**: 1536
- **Max Input**: 8192 tokens
- **Cost**: ~$0.02 per 1M tokens (very cost-effective)
- **Performance**: Excellent for code and documentation search

## Environment Variables

The provider supports multiple environment variable formats:

```bash
OPENAI_API_KEY=sk_your-key        # Primary
OPENAI_KEY=sk_your-key           # Alternative
OpenAI__ApiKey=sk_your-key       # Configuration format
```

## Error Handling

The provider includes comprehensive error handling:

- **Invalid API key** - Clear error messages
- **Rate limiting** - Proper error propagation
- **Network issues** - Retry-friendly design
- **Health checks** - Built-in connectivity testing

## Usage Examples

### Basic Embedding Generation
```csharp
public class MyService
{
    private readonly IEmbeddingProvider _embeddingProvider;
    
    public MyService(IEmbeddingProvider embeddingProvider)
    {
        _embeddingProvider = embeddingProvider;
    }
    
    public async Task<float[]> GetEmbedding(string text)
    {
        return await _embeddingProvider.GenerateEmbeddingAsync(text);
    }
}
```

### Batch Processing
```csharp
var texts = new[] { "text1", "text2", "text3" };
var embeddings = await embeddingProvider.GenerateEmbeddingsAsync(texts);
```

### Health Check
```csharp
var isHealthy = await embeddingProvider.HealthCheckAsync();
if (!isHealthy)
{
    // Handle provider unavailability
}
```

## Cost Optimization

- **Batch requests** - Use `GenerateEmbeddingsAsync()` for multiple texts
- **Cache embeddings** - Store results to avoid re-computation
- **Monitor usage** - Track API costs through OpenAI dashboard
- **Chunking strategy** - Optimize text chunk sizes for best quality/cost ratio

## Alternative Embedding Providers

For different use cases, consider:

- **`Agentix.Rag.Embeddings.Claude`** 🚧 - Coming soon (when Claude supports embeddings)
- **`Agentix.Rag.Embeddings.Azure`** 🚧 - Azure OpenAI embeddings
- **`Agentix.Rag.Embeddings.Local`** 🚧 - Local/self-hosted models

## Provider Comparison

| Provider | Cost | Quality | Speed | Use Case |
|----------|------|---------|-------|----------|
| OpenAI Small | 💰 | ⭐⭐⭐ | ⚡⚡⚡ | General purpose, cost-effective |
| OpenAI Large | 💰💰 | ⭐⭐⭐⭐ | ⚡⚡ | High-quality search, premium use |
| Azure OpenAI | 💰💰 | ⭐⭐⭐ | ⚡⚡ | Enterprise, data residency |

## Troubleshooting

### "Invalid API key"
- Verify your OpenAI API key format (starts with `sk-`)
- Check environment variable is set correctly
- Ensure API key has sufficient credits

### "Rate limit exceeded"
- Implement exponential backoff in your application
- Consider upgrading your OpenAI plan
- Use batch processing to reduce request frequency

### "Model not found"
- The provider uses `text-embedding-3-small` by default
- Ensure your OpenAI account has access to embedding models
- Check OpenAI service status

## License

MIT License - build awesome AI apps! 🚀 