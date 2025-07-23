using System.Text;
using Agentix.Rag.Core.Interfaces;
using Agentix.Rag.Embeddings.Local.Models;
using Agentix.Rag.Embeddings.Local.Tokenization;
using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;

namespace Agentix.Rag.Embeddings.Local;

/// <summary>
/// Local embedding provider using ONNX Runtime for offline text embeddings.
/// Uses the all-MiniLM-L6-v2 model for fast, quality embeddings without external dependencies.
/// </summary>
public class LocalEmbeddingProvider : IEmbeddingProvider, IDisposable
{
    private readonly ModelManager _modelManager;
    private readonly BertTokenizer _tokenizer;
    private readonly ILogger<LocalEmbeddingProvider> _logger;
    private readonly SemaphoreSlim _semaphore;
    private InferenceSession? _session;
    private bool _disposed;

    public string Name => "Local-AllMiniLM-L6-v2";
    public int EmbeddingDimension => 384; // all-MiniLM-L6-v2 dimension

    public LocalEmbeddingProvider(ModelManager modelManager, BertTokenizer tokenizer, ILogger<LocalEmbeddingProvider> logger)
    {
        _modelManager = modelManager ?? throw new ArgumentNullException(nameof(modelManager));
        _tokenizer = tokenizer ?? throw new ArgumentNullException(nameof(tokenizer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _semaphore = new SemaphoreSlim(1, 1); // Thread-safe model access
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        try
        {
            await EnsureModelLoadedAsync(cancellationToken);
            return await GenerateEmbeddingInternalAsync(text, cancellationToken);
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            _logger.LogError(ex, "Error generating embedding for text of length {Length}", text.Length);
            throw;
        }
    }

    public async Task<IEnumerable<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        var textArray = texts.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
        
        if (textArray.Length == 0)
        {
            return Enumerable.Empty<float[]>();
        }

        try
        {
            await EnsureModelLoadedAsync(cancellationToken);
            
            // Process in batches for memory efficiency
            var results = new List<float[]>();
            const int batchSize = 32;
            
            for (int i = 0; i < textArray.Length; i += batchSize)
            {
                var batch = textArray.Skip(i).Take(batchSize);
                var batchResults = await Task.WhenAll(
                    batch.Select(text => GenerateEmbeddingInternalAsync(text, cancellationToken))
                );
                results.AddRange(batchResults);
            }
            
            return results;
        }
        catch (Exception ex) when (!(ex is OperationCanceledException))
        {
            _logger.LogError(ex, "Error generating embeddings for {Count} texts", textArray.Length);
            throw;
        }
    }

    public async Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Simple health check with a small embedding request
            await GenerateEmbeddingAsync("test", cancellationToken);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Local embedding provider health check failed");
            return false;
        }
    }

    private async Task EnsureModelLoadedAsync(CancellationToken cancellationToken)
    {
        if (_session != null) return;

        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_session != null) return; // Double-check after acquiring lock

            _logger.LogInformation("Loading ONNX model for local embeddings...");
            
            var modelPath = await _modelManager.EnsureModelAvailableAsync(cancellationToken);
            _session = new InferenceSession(modelPath);
            
            _logger.LogInformation("ONNX model loaded successfully from {ModelPath}", modelPath);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async Task<float[]> GenerateEmbeddingInternalAsync(string text, CancellationToken cancellationToken)
    {
        if (_session == null)
        {
            throw new InvalidOperationException("Model not loaded");
        }

        // Tokenize the text
        var tokens = _tokenizer.Tokenize(text);
        var inputIds = tokens.InputIds;
        var attentionMask = tokens.AttentionMask;

        // Prepare ONNX inputs
        var inputIdsTensor = new DenseTensor<long>(inputIds.Select(x => (long)x).ToArray(), new[] { 1, inputIds.Length });
        var attentionMaskTensor = new DenseTensor<long>(attentionMask.Select(x => (long)x).ToArray(), new[] { 1, attentionMask.Length });
        var tokenTypeIdsTensor = new DenseTensor<long>(new long[inputIds.Length], new[] { 1, inputIds.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIdsTensor),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMaskTensor),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIdsTensor)
        };

        // Run inference
        return await Task.Run(() =>
        {
            using var results = _session.Run(inputs);
            
            // Log available outputs for debugging
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                var outputNames = string.Join(", ", results.Select(r => r.Name));
                _logger.LogDebug("Available model outputs: {OutputNames}", outputNames);
            }
            
            // Try to find the output tensor - different models may have different output names
            var outputTensor = results.FirstOrDefault(x => x.Name == "last_hidden_state") ??
                              results.FirstOrDefault(x => x.Name == "hidden_states") ??
                              results.FirstOrDefault(x => x.Name.Contains("hidden")) ??
                              results.FirstOrDefault(); // Fallback to first output
            
            if (outputTensor == null)
            {
                throw new InvalidOperationException($"No suitable output tensor found. Available outputs: {string.Join(", ", results.Select(r => r.Name))}");
            }
            
            var lastHiddenState = outputTensor.AsEnumerable<float>().ToArray();
            
            // Apply mean pooling with attention mask
            var embedding = ApplyMeanPooling(lastHiddenState, attentionMask, EmbeddingDimension);
            
            // Normalize the embedding
            return NormalizeVector(embedding);
        }, cancellationToken);
    }

    private static float[] ApplyMeanPooling(float[] lastHiddenState, int[] attentionMask, int embeddingDim)
    {
        var seqLength = attentionMask.Length;
        var embedding = new float[embeddingDim];
        var validTokens = 0;

        // Sum embeddings for valid tokens
        for (int i = 0; i < seqLength; i++)
        {
            if (attentionMask[i] == 1)
            {
                validTokens++;
                for (int j = 0; j < embeddingDim; j++)
                {
                    embedding[j] += lastHiddenState[i * embeddingDim + j];
                }
            }
        }

        // Average the embeddings
        if (validTokens > 0)
        {
            for (int i = 0; i < embeddingDim; i++)
            {
                embedding[i] /= validTokens;
            }
        }

        return embedding;
    }

    private static float[] NormalizeVector(float[] vector)
    {
        var magnitude = Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= (float)magnitude;
            }
        }
        return vector;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _session?.Dispose();
                _semaphore?.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
} 