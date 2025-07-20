using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

public interface IAgentixOrchestrator
{
    Task<AIResponse> ProcessMessageAsync(IncomingMessage message, CancellationToken cancellationToken = default);
    Task<AIResponse> ProcessMessageAsync(IncomingMessage message, string? providerName = null, CancellationToken cancellationToken = default);
    
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    // Health and status
    Task<bool> IsHealthyAsync();
    Task<SystemStatus> GetStatusAsync();
}

public class SystemStatus
{
    public bool IsHealthy { get; set; }
    public int RegisteredProviders { get; set; }
    public int RunningChannels { get; set; }
    public Dictionary<string, bool> ProviderHealth { get; set; } = new();
    public Dictionary<string, bool> ChannelStatus { get; set; } = new();
    public DateTime LastChecked { get; set; } = DateTime.UtcNow;
} 