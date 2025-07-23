using System.Text.Json;
using Agentix.Rag.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.Embeddings.OpenAI;

/// <summary>
/// OpenAI embedding provider for generating text embeddings.
/// Uses the text-embedding-3-small model for cost-effective embeddings.
/// </summary>
public class OpenAIEmbeddingProvider : IEmbeddingProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly ILogger<OpenAIEmbeddingProvider> _logger;
    private const string DefaultModel = "text-embedding-3-small";
    private const string ApiBaseUrl = "https://api.openai.com/v1";

    public string Name => "OpenAI";
    public int EmbeddingDimension => 1536; // text-embedding-3-small dimension

    public OpenAIEmbeddingProvider(string apiKey, HttpClient httpClient, ILogger<OpenAIEmbeddingProvider> logger)
    {
        _apiKey = apiKey ?? throw new ArgumentNullException(nameof(apiKey));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger;
        
        // Configure HTTP client for OpenAI API
        _httpClient.BaseAddress = new Uri(ApiBaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Agentix.Rag.Embeddings.OpenAI/1.0");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Text cannot be null or empty", nameof(text));
        }

        try
        {
            var request = new
            {
                model = DefaultModel,
                input = text.Trim(),
                encoding_format = "float"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/embeddings", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new InvalidOperationException($"OpenAI API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

            if (responseData?.Data?.Length > 0)
            {
                return responseData.Data[0].Embedding;
            }

            throw new InvalidOperationException("No embedding data received from OpenAI API");
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
            var request = new
            {
                model = DefaultModel,
                input = textArray.Select(t => t.Trim()).ToArray(),
                encoding_format = "float"
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/embeddings", content, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("OpenAI API error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                throw new InvalidOperationException($"OpenAI API error: {response.StatusCode}");
            }

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var responseData = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

            if (responseData?.Data?.Length > 0)
            {
                return responseData.Data
                    .OrderBy(d => d.Index)
                    .Select(d => d.Embedding);
            }

            throw new InvalidOperationException("No embedding data received from OpenAI API");
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
            _logger.LogWarning(ex, "OpenAI embedding provider health check failed");
            return false;
        }
    }

    private class OpenAIEmbeddingResponse
    {
        public string Object { get; set; } = string.Empty;
        public OpenAIEmbeddingData[] Data { get; set; } = Array.Empty<OpenAIEmbeddingData>();
        public string Model { get; set; } = string.Empty;
        public OpenAIUsage Usage { get; set; } = new();
    }

    private class OpenAIEmbeddingData
    {
        public string Object { get; set; } = string.Empty;
        public int Index { get; set; }
        public float[] Embedding { get; set; } = Array.Empty<float>();
    }

    private class OpenAIUsage
    {
        public int PromptTokens { get; set; }
        public int TotalTokens { get; set; }
    }
} 