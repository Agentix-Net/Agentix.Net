using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

public interface IChannelAdapter
{
    string Name { get; }
    string ChannelType { get; }
    bool IsRunning { get; }
    
    Task StartAsync(CancellationToken cancellationToken = default);
    Task StopAsync(CancellationToken cancellationToken = default);
    
    Task<bool> CanHandleAsync(IncomingMessage message);
    Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default);
    Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default);
    
    // Channel-specific capabilities
    bool SupportsRichContent { get; }
    bool SupportsFileUploads { get; }
    bool SupportsInteractiveElements { get; }
} 