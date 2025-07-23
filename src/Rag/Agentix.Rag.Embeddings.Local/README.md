# Agentix.Rag.Embeddings.Local

Local ONNX-based embedding provider for Agentix.Net RAG Engine. Provides zero-cost, offline text embeddings without external API dependencies using Microsoft ONNX Runtime.

## Overview

This package enables completely local text embedding generation using pre-trained ONNX models. Perfect for:

- **Cost-sensitive applications** - Zero ongoing costs after setup
- **Privacy-first scenarios** - All processing stays local
- **Offline environments** - Works without internet connectivity
- **Development and testing** - No API keys or rate limits

## Features

- ✅ **Zero external dependencies** - No API calls after model download
- ✅ **Microsoft ONNX Runtime** - Enterprise-grade inference engine
- ✅ **Automatic model management** - Downloads and caches models locally
- ✅ **Multiple model support** - all-MiniLM-L6-v2, all-mpnet-base-v2
- ✅ **Thread-safe** - Concurrent embedding generation
- ✅ **Batch processing** - Efficient multi-text embedding
- ✅ **Memory efficient** - Smart model loading and caching

## Supported Models

| Model | Dimensions | Description | Download Size |
|-------|------------|-------------|---------------|
| all-MiniLM-L6-v2 | 384 | Fast, good quality (default) | ~23MB |
| all-mpnet-base-v2 | 768 | Better quality, slower | ~125MB |

## Installation

```bash
dotnet add package Agentix.Rag.Embeddings.Local
```

## Quick Start

### Basic Usage

```csharp
using Agentix.Rag.Embeddings.Local.Extensions;
using Microsoft.Extensions.DependencyInjection;

// Register local embeddings with default settings
services.AddLocalEmbeddings();

// Use in your application
var provider = serviceProvider.GetRequiredService<IEmbeddingProvider>();
var embedding = await provider.GenerateEmbeddingAsync("Hello world");
```

### RAG Engine Integration

```csharp
using Agentix.Rag.Embeddings.Local.Extensions;
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.InMemory.Extensions;

services.AddRAGEngine()
    .AddGitHubSource(options => {
        options.Repository = "myorg/myrepo";
        options.AccessToken = "github_token";
    })
    .AddInMemoryVectorStore()
    .AddLocalEmbeddings();  // Zero-cost embeddings!
```

## Configuration Options

### Custom Cache Directory

```csharp
services.AddLocalEmbeddings(cacheDirectory: @"C:\MyApp\Models");
```

### Model Selection

```csharp
services.AddLocalEmbeddings("all-mpnet-base-v2"); // Higher quality model
```

### Advanced Configuration

```csharp
services.AddLocalEmbeddings(options => {
    options.ModelName = "all-MiniLM-L6-v2";
    options.CacheDirectory = @"C:\MyApp\Models";
    options.MaxSequenceLength = 512;
    options.AutoDownloadModels = true;
});
```

## Model Management

### Automatic Download

Models are automatically downloaded from Hugging Face on first use:

```csharp
// First call will download model (~23MB for all-MiniLM-L6-v2)
var embedding = await provider.GenerateEmbeddingAsync("test");
```

### Cache Location

By default, models are cached in:
- **Windows**: `%USERPROFILE%\.agentix\models\`
- **Linux/Mac**: `~/.agentix/models/`

### Manual Model Management

```csharp
var modelManager = serviceProvider.GetRequiredService<ModelManager>();

// Check cached models
var models = await modelManager.GetCachedModelsAsync();
foreach (var model in models)
{
    Console.WriteLine($"{model.Name}: {model.SizeBytes:N0} bytes");
}

// Clear cache if needed
await modelManager.ClearCacheAsync();
```

## Performance

### Benchmarks (all-MiniLM-L6-v2)

| Operation | Time | Notes |
|-----------|------|-------|
| Cold start | ~2-3s | First model load |
| Single embedding | ~10-50ms | Depends on text length |
| Batch (10 texts) | ~100-200ms | More efficient |
| Warm inference | ~5-15ms | Model already loaded |

### Memory Usage

- **Model loading**: ~150MB RAM
- **Per embedding**: ~1-5MB temporary
- **Cached model**: ~23MB disk space

## Error Handling

### Common Issues

**Model download fails:**
```csharp
try 
{
    var embedding = await provider.GenerateEmbeddingAsync("test");
}
catch (InvalidOperationException ex) when (ex.Message.Contains("download"))
{
    // Handle network issues
    logger.LogError("Failed to download model: {Error}", ex.Message);
}
```

**Insufficient disk space:**
```csharp
services.AddLocalEmbeddings(options => {
    options.CacheDirectory = @"D:\LargerDrive\Models"; // Use different drive
});
```

## Integration Examples

### With ASP.NET Core

```csharp
// Program.cs
builder.Services.AddRAGEngine()
    .AddGitHubSource(options => options.Repository = "myorg/myrepo")
    .AddInMemoryVectorStore()
    .AddLocalEmbeddings();

// Controller
[ApiController]
public class SearchController : ControllerBase
{
    private readonly IRAGEngine _ragEngine;
    
    public SearchController(IRAGEngine ragEngine)
    {
        _ragEngine = ragEngine;
    }
    
    [HttpGet("search")]
    public async Task<IActionResult> Search(string query)
    {
        var results = await _ragEngine.SearchAsync(query);
        return Ok(results);
    }
}
```

### Health Checks

```csharp
services.AddHealthChecks()
    .AddCheck<LocalEmbeddingHealthCheck>("local-embeddings");

// Custom health check
public class LocalEmbeddingHealthCheck : IHealthCheck
{
    private readonly IEmbeddingProvider _provider;
    
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var healthy = await _provider.HealthCheckAsync(cancellationToken);
            return healthy ? HealthCheckResult.Healthy() : HealthCheckResult.Unhealthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message);
        }
    }
}
```

## Comparison with Other Providers

| Feature | Local | OpenAI | Voyage |
|---------|-------|--------|--------|
| **Cost** | Free | $0.10/1M tokens | $0.10/1M tokens |
| **Latency** | 10-50ms | 100-500ms | 100-500ms |
| **Privacy** | ✅ Local | ❌ External | ❌ External |
| **Offline** | ✅ Yes | ❌ No | ❌ No |
| **Quality** | Good | Excellent | Excellent* |
| **Setup** | Auto | API Key | API Key |

*Voyage is optimized for Claude

## Migration Guide

### From OpenAI Embeddings

```csharp
// Before
services.AddOpenAIEmbeddings("your-api-key");

// After
services.AddLocalEmbeddings();
// Note: Different dimensions (384 vs 1536) - will need to re-index
```

### Dimension Compatibility

**Important**: Local embeddings use 384 dimensions (vs OpenAI's 1536). You'll need to:

1. Clear existing vector store
2. Re-index documents with new embeddings
3. Update any hardcoded dimension references

## Troubleshooting

### Common Solutions

**Q: Model download is slow**
A: Models are downloaded once and cached. Subsequent runs are instant.

**Q: High memory usage**
A: Model stays loaded for performance. Memory is released when provider is disposed.

**Q: Different results than OpenAI**
A: Expected - different models produce different embeddings. Quality is good but not identical.

**Q: Tokenization errors**
A: Current tokenizer is simplified. For production, consider using proper BERT tokenization.

### Logging

```csharp
// Enable detailed logging
services.AddLogging(builder => {
    builder.AddConsole();
    builder.SetMinimumLevel(LogLevel.Debug);
});
```

## Roadmap

- [ ] **Better tokenization** - Proper BERT WordPiece tokenizer
- [ ] **More models** - Support for specialized models
- [ ] **GPU acceleration** - ONNX Runtime GPU providers
- [ ] **Quantized models** - Smaller, faster models
- [ ] **Model warm-up** - Pre-load models on startup

## Contributing

See [CONTRIBUTING.md](../../../CONTRIBUTING.md) for guidelines on extending the local embedding system.

## License

This package is licensed under the MIT License. 