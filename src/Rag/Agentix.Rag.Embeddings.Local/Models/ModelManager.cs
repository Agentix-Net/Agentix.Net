using System.IO.Compression;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Agentix.Rag.Embeddings.Local.Models;

/// <summary>
/// Manages ONNX model downloading, caching, and loading for local embeddings.
/// </summary>
public class ModelManager
{
    private readonly ILogger<ModelManager> _logger;
    private readonly string _cacheDirectory;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _downloadSemaphore;

    public const string DefaultModelName = "all-MiniLM-L6-v2";
    public const string DefaultHuggingFaceRepo = "onnx-models/all-MiniLM-L6-v2-onnx";

    public ModelManager(ILogger<ModelManager> logger, HttpClient httpClient, string? cacheDirectory = null)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _cacheDirectory = cacheDirectory ?? GetDefaultCacheDirectory();
        _downloadSemaphore = new SemaphoreSlim(1, 1);
        
        // Ensure cache directory exists
        Directory.CreateDirectory(_cacheDirectory);
    }

    /// <summary>
    /// Ensures the ONNX model is available locally, downloading if necessary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to the local ONNX model file</returns>
    public async Task<string> EnsureModelAvailableAsync(CancellationToken cancellationToken = default)
    {
        return await EnsureModelAvailableAsync(DefaultModelName, cancellationToken);
    }

    /// <summary>
    /// Ensures the specified ONNX model is available locally, downloading if necessary.
    /// </summary>
    /// <param name="modelName">Name of the model to ensure is available</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Path to the local ONNX model file</returns>
    public async Task<string> EnsureModelAvailableAsync(string modelName, CancellationToken cancellationToken = default)
    {
        var modelDirectory = Path.Combine(_cacheDirectory, modelName);
        var modelPath = Path.Combine(modelDirectory, "model.onnx");
        var metadataPath = Path.Combine(modelDirectory, "metadata.json");

        // Check if model already exists and is valid
        if (File.Exists(modelPath) && File.Exists(metadataPath))
        {
            _logger.LogDebug("Model {ModelName} found in cache at {ModelPath}", modelName, modelPath);
            return modelPath;
        }

        // Download model if not found
        await _downloadSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring semaphore
            if (File.Exists(modelPath) && File.Exists(metadataPath))
            {
                return modelPath;
            }

            _logger.LogInformation("Downloading model {ModelName} to {ModelDirectory}", modelName, modelDirectory);
            await DownloadModelAsync(modelName, modelDirectory, cancellationToken);
            
            // Create metadata
            var metadata = new ModelMetadata
            {
                ModelName = modelName,
                DownloadedAt = DateTime.UtcNow,
                Version = "1.0.0"
            };
            
            await File.WriteAllTextAsync(metadataPath, JsonSerializer.Serialize(metadata, new JsonSerializerOptions { WriteIndented = true }), cancellationToken);
            
            _logger.LogInformation("Model {ModelName} downloaded successfully", modelName);
            return modelPath;
        }
        finally
        {
            _downloadSemaphore.Release();
        }
    }

    /// <summary>
    /// Downloads the ONNX model from Hugging Face.
    /// </summary>
    private async Task DownloadModelAsync(string modelName, string targetDirectory, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(targetDirectory);

        string repoUrl = GetHuggingFaceRepoUrl(modelName);
        string modelUrl = $"{repoUrl}/resolve/main/model.onnx";

        _logger.LogDebug("Downloading from {ModelUrl}", modelUrl);

        try
        {
            var response = await _httpClient.GetAsync(modelUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            var modelPath = Path.Combine(targetDirectory, "model.onnx");
            await using var fileStream = new FileStream(modelPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream, cancellationToken);

            _logger.LogDebug("Model downloaded to {ModelPath}, size: {Size} bytes", modelPath, new FileInfo(modelPath).Length);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to download model from {ModelUrl}", modelUrl);
            throw new InvalidOperationException($"Failed to download model {modelName} from Hugging Face", ex);
        }
    }

    /// <summary>
    /// Gets the Hugging Face repository URL for the given model.
    /// </summary>
    private static string GetHuggingFaceRepoUrl(string modelName)
    {
        return modelName switch
        {
            "all-MiniLM-L6-v2" => "https://huggingface.co/onnx-models/all-MiniLM-L6-v2-onnx",
            "all-mpnet-base-v2" => "https://huggingface.co/onnx-models/all-mpnet-base-v2-onnx",
            _ => throw new ArgumentException($"Unsupported model: {modelName}")
        };
    }

    /// <summary>
    /// Gets the default cache directory for models.
    /// </summary>
    private static string GetDefaultCacheDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".agentix", "models");
    }

    /// <summary>
    /// Clears the model cache.
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await Task.Run(() =>
        {
            if (Directory.Exists(_cacheDirectory))
            {
                Directory.Delete(_cacheDirectory, recursive: true);
                Directory.CreateDirectory(_cacheDirectory);
                _logger.LogInformation("Model cache cleared: {CacheDirectory}", _cacheDirectory);
            }
        });
    }

    /// <summary>
    /// Gets information about cached models.
    /// </summary>
    public async Task<IEnumerable<ModelInfo>> GetCachedModelsAsync()
    {
        return await Task.Run(() =>
        {
            var models = new List<ModelInfo>();
            
            if (!Directory.Exists(_cacheDirectory))
                return models;

            foreach (var modelDir in Directory.GetDirectories(_cacheDirectory))
            {
                var modelName = Path.GetFileName(modelDir);
                var modelPath = Path.Combine(modelDir, "model.onnx");
                var metadataPath = Path.Combine(modelDir, "metadata.json");

                if (File.Exists(modelPath))
                {
                    var info = new ModelInfo
                    {
                        Name = modelName,
                        Path = modelPath,
                        SizeBytes = new FileInfo(modelPath).Length,
                        LastModified = File.GetLastWriteTime(modelPath)
                    };

                    if (File.Exists(metadataPath))
                    {
                        try
                        {
                            var metadataJson = File.ReadAllText(metadataPath);
                            var metadata = JsonSerializer.Deserialize<ModelMetadata>(metadataJson);
                            info.DownloadedAt = metadata?.DownloadedAt;
                            info.Version = metadata?.Version;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Failed to read metadata for model {ModelName}", modelName);
                        }
                    }

                    models.Add(info);
                }
            }

            return models;
        });
    }
}

/// <summary>
/// Metadata for a cached model.
/// </summary>
public class ModelMetadata
{
    public string ModelName { get; set; } = string.Empty;
    public DateTime DownloadedAt { get; set; }
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// Information about a cached model.
/// </summary>
public class ModelInfo
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public DateTime LastModified { get; set; }
    public DateTime? DownloadedAt { get; set; }
    public string? Version { get; set; }
} 