using System.Diagnostics;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Core.Models;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.GitHub;

/// <summary>
/// Main RAG engine implementation for GitHub repositories.
/// Coordinates document loading, embedding generation, and search operations.
/// </summary>
internal class GitHubRAGEngine : IRAGEngine, IHostedService
{
    private readonly IDocumentSource _documentSource;
    private readonly IVectorStore _vectorStore;
    private readonly IEmbeddingProvider _embeddingProvider;
    private readonly GitHubRAGOptions _options;
    private readonly ILogger<GitHubRAGEngine> _logger;
    
    private readonly List<SourceConfig> _sourceConfigs = new();
    private bool _isReady = false;
    private Timer? _syncTimer;

    public GitHubRAGEngine(
        IDocumentSource documentSource,
        IVectorStore vectorStore,
        IEmbeddingProvider embeddingProvider,
        GitHubRAGOptions options,
        ILogger<GitHubRAGEngine> logger)
    {
        _documentSource = documentSource;
        _vectorStore = vectorStore;
        _embeddingProvider = embeddingProvider;
        _options = options;
        _logger = logger;
        
        // Create source configurations from repository URLs
        CreateSourceConfigurations();
    }

    public async Task<RAGResult> SearchAsync(string query, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            if (!_isReady)
            {
                _logger.LogWarning("RAG engine not ready, returning empty results");
                return new RAGResult
                {
                    Query = query,
                    Documents = Array.Empty<DocumentResult>(),
                    TotalResults = 0,
                    QueryTime = stopwatch.Elapsed
                };
            }

            // Generate embedding for the query
            var queryEmbedding = await _embeddingProvider.GenerateEmbeddingAsync(query, cancellationToken);
            
            // Search for similar documents
            var searchResults = await _vectorStore.SearchSimilarAsync(
                queryEmbedding, 
                maxResults * 2, // Get more results to filter and rank
                0.6, // Lower threshold for initial search
                cancellationToken);
            
            // Convert to document results
            var documentResults = searchResults
                .Take(maxResults)
                .Select(result => new DocumentResult
                {
                    Content = TruncateContent(result.Document.Content, 500),
                    Title = result.Document.Title,
                    Repository = result.Document.SourceName,
                    FilePath = result.Document.Path ?? "",
                    Url = result.Document.Url,
                    Similarity = result.Similarity,
                    Type = result.Document.Type,
                    LastModified = result.Document.LastModified
                })
                .ToArray();

            stopwatch.Stop();
            
            _logger.LogInformation("Search for '{Query}' returned {Count} results in {Duration}ms", 
                                 query, documentResults.Length, stopwatch.ElapsedMilliseconds);

            return new RAGResult
            {
                Query = query,
                Documents = documentResults,
                TotalResults = searchResults.Count(),
                QueryTime = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for query: {Query}", query);
            
            return new RAGResult
            {
                Query = query,
                Documents = Array.Empty<DocumentResult>(),
                TotalResults = 0,
                QueryTime = stopwatch.Elapsed
            };
        }
    }

    public async Task IndexDocumentsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting document indexing for {Count} repositories", _sourceConfigs.Count);
        
        try
        {
            var totalDocuments = 0;
            
            foreach (var sourceConfig in _sourceConfigs)
            {
                if (cancellationToken.IsCancellationRequested) break;
                
                try
                {
                    _logger.LogInformation("Indexing source: {SourceName}", sourceConfig.Name);
                    
                    // Load documents from source
                    var documents = await _documentSource.LoadDocumentsAsync(sourceConfig, cancellationToken);
                    var documentList = documents.ToList();
                    
                    if (documentList.Count == 0)
                    {
                        _logger.LogWarning("No documents loaded from source: {SourceName}", sourceConfig.Name);
                        continue;
                    }
                    
                    // Generate embeddings in batches
                    const int batchSize = 20; // Process in smaller batches to avoid API limits
                    var embeddings = new List<DocumentEmbedding>();
                    
                    for (int i = 0; i < documentList.Count; i += batchSize)
                    {
                        if (cancellationToken.IsCancellationRequested) break;
                        
                        var batch = documentList.Skip(i).Take(batchSize).ToList();
                        var texts = batch.Select(d => d.Content).ToArray();
                        
                        try
                        {
                            var batchEmbeddings = await _embeddingProvider.GenerateEmbeddingsAsync(texts, cancellationToken);
                            var embeddingArray = batchEmbeddings.ToArray();
                            
                            for (int j = 0; j < batch.Count && j < embeddingArray.Length; j++)
                            {
                                embeddings.Add(new DocumentEmbedding
                                {
                                    Document = batch[j],
                                    Embedding = embeddingArray[j]
                                });
                            }
                            
                            _logger.LogDebug("Generated embeddings for batch {BatchStart}-{BatchEnd} of {Total}", 
                                           i + 1, Math.Min(i + batchSize, documentList.Count), documentList.Count);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error generating embeddings for batch {BatchStart}-{BatchEnd}", 
                                             i + 1, Math.Min(i + batchSize, documentList.Count));
                        }
                        
                        // Small delay between batches to respect API rate limits
                        await Task.Delay(100, cancellationToken);
                    }
                    
                    // Store embeddings
                    if (embeddings.Count > 0)
                    {
                        await _vectorStore.StoreEmbeddingsAsync(embeddings, cancellationToken);
                        totalDocuments += embeddings.Count;
                        
                        _logger.LogInformation("Indexed {Count} documents from {SourceName}", 
                                             embeddings.Count, sourceConfig.Name);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error indexing source: {SourceName}", sourceConfig.Name);
                }
            }
            
            _isReady = totalDocuments > 0;
            
            _logger.LogInformation("Document indexing completed. Total documents: {Total}, Ready: {Ready}", 
                                 totalDocuments, _isReady);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during document indexing");
        }
    }

    public async Task<SourceStatus[]> GetSourceStatusAsync()
    {
        var statuses = new List<SourceStatus>();
        
        foreach (var sourceConfig in _sourceConfigs)
        {
            try
            {
                var status = await _documentSource.GetStatusAsync(sourceConfig);
                statuses.Add(status);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error getting status for source: {SourceName}", sourceConfig.Name);
                
                statuses.Add(new SourceStatus
                {
                    SourceId = sourceConfig.Id,
                    SourceName = sourceConfig.Name ?? "Unknown",
                    Status = IndexingStatus.Error,
                    ErrorMessage = ex.Message
                });
            }
        }
        
        return statuses.ToArray();
    }

    public Task<bool> IsReadyAsync()
    {
        return Task.FromResult(_isReady);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting GitHub RAG engine with {Count} repositories", _options.Repositories.Length);
        
        // Start initial indexing in background
        _ = Task.Run(async () =>
        {
            try
            {
                await IndexDocumentsAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during initial indexing");
            }
        }, cancellationToken);
        
        // Set up periodic sync timer
        _syncTimer = new Timer(
            async _ => await PeriodicSync(), 
            null, 
            _options.SyncInterval, 
            _options.SyncInterval);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping GitHub RAG engine");
        
        _syncTimer?.Dispose();
        return Task.CompletedTask;
    }

    private void CreateSourceConfigurations()
    {
        foreach (var repoUrl in _options.Repositories)
        {
            var sourceConfig = new SourceConfig
            {
                Id = GenerateSourceId(repoUrl),
                SourceType = "github",
                Name = ExtractRepoName(repoUrl),
                Configuration = new Dictionary<string, object>
                {
                    ["url"] = repoUrl
                }
            };
            
            _sourceConfigs.Add(sourceConfig);
        }
        
        _logger.LogDebug("Created {Count} source configurations", _sourceConfigs.Count);
    }

    private static string GenerateSourceId(string repoUrl)
    {
        // Create a consistent ID from the repo URL
        return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(repoUrl))
            .Replace('/', '_')
            .Replace('+', '-')
            .TrimEnd('=');
    }

    private static string ExtractRepoName(string repoUrl)
    {
        try
        {
            var uri = new Uri(repoUrl);
            var path = uri.AbsolutePath.Trim('/');
            return path.Contains('/') ? path : repoUrl;
        }
        catch
        {
            return repoUrl;
        }
    }

    private static string TruncateContent(string content, int maxLength)
    {
        if (content.Length <= maxLength)
            return content;
        
        var truncated = content.Substring(0, maxLength);
        var lastSpace = truncated.LastIndexOf(' ');
        
        if (lastSpace > maxLength * 0.8) // If we can find a good breaking point
        {
            return truncated.Substring(0, lastSpace) + "...";
        }
        
        return truncated + "...";
    }

    private async Task PeriodicSync()
    {
        try
        {
            _logger.LogDebug("Starting periodic sync");
            await IndexDocumentsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error during periodic sync");
        }
    }
} 