using System.Collections.Concurrent;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Core.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.InMemory;

/// <summary>
/// Simple in-memory vector store implementation.
/// Suitable for development and small-scale deployments.
/// Uses cosine similarity for vector search.
/// </summary>
public class InMemoryVectorStore : IVectorStore
{
    private readonly ConcurrentDictionary<string, StoredEmbedding> _embeddings = new();
    private readonly ILogger<InMemoryVectorStore> _logger;

    public InMemoryVectorStore(ILogger<InMemoryVectorStore> logger)
    {
        _logger = logger;
    }

    public Task StoreEmbeddingsAsync(IEnumerable<DocumentEmbedding> embeddings, CancellationToken cancellationToken = default)
    {
        foreach (var embedding in embeddings)
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            var stored = new StoredEmbedding
            {
                Document = embedding.Document,
                Embedding = embedding.Embedding,
                CreatedAt = embedding.CreatedAt
            };
            
            _embeddings.AddOrUpdate(embedding.Document.Id, stored, (key, existing) => stored);
        }
        
        _logger.LogDebug("Stored {Count} embeddings in memory", embeddings.Count());
        return Task.CompletedTask;
    }

    public Task<IEnumerable<VectorSearchResult>> SearchSimilarAsync(
        float[] queryEmbedding, 
        int maxResults = 10, 
        double similarityThreshold = 0.7, 
        CancellationToken cancellationToken = default)
    {
        var results = new List<VectorSearchResult>();
        
        foreach (var kvp in _embeddings)
        {
            if (cancellationToken.IsCancellationRequested) break;
            
            var stored = kvp.Value;
            var similarity = CalculateCosineSimilarity(queryEmbedding, stored.Embedding);
            
            if (similarity >= similarityThreshold)
            {
                results.Add(new VectorSearchResult
                {
                    Document = stored.Document,
                    Similarity = similarity,
                    Distance = 1.0 - similarity
                });
            }
        }
        
        // Sort by similarity (highest first) and take max results
        var sortedResults = results
            .OrderByDescending(r => r.Similarity)
            .Take(maxResults);
        
        _logger.LogDebug("Found {Count} similar documents above threshold {Threshold}", 
                        sortedResults.Count(), similarityThreshold);
        
        return Task.FromResult(sortedResults);
    }

    public Task DeleteBySourceAsync(string sourceId, CancellationToken cancellationToken = default)
    {
        var keysToRemove = _embeddings
            .Where(kvp => kvp.Value.Document.SourceId == sourceId)
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _embeddings.TryRemove(key, out _);
        }
        
        _logger.LogInformation("Removed {Count} embeddings for source {SourceId}", keysToRemove.Count, sourceId);
        return Task.CompletedTask;
    }

    public Task<int> GetDocumentCountAsync()
    {
        return Task.FromResult(_embeddings.Count);
    }

    public Task ClearAsync(CancellationToken cancellationToken = default)
    {
        var count = _embeddings.Count;
        _embeddings.Clear();
        
        _logger.LogInformation("Cleared {Count} embeddings from memory", count);
        return Task.CompletedTask;
    }

    private static double CalculateCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length)
            throw new ArgumentException("Vector dimensions must match");
        
        double dotProduct = 0.0;
        double normA = 0.0;
        double normB = 0.0;
        
        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }
        
        if (normA == 0.0 || normB == 0.0)
            return 0.0;
        
        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    private class StoredEmbedding
    {
        public Document Document { get; set; } = null!;
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public DateTime CreatedAt { get; set; }
    }
} 