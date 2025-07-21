namespace Agentix.Providers.Claude;

public class ClaudeOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    public string DefaultModel { get; set; } = "claude-3-haiku-20240307";
    public float Temperature { get; set; } = 0.7f;
    public int MaxTokens { get; set; } = 1000;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableRetry { get; set; } = true;
    public int MaxRetries { get; set; } = 3;
    public int MaxHistoryMessages { get; set; } = 10;
} 