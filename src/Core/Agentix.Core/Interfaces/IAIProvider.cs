using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

public interface IAIProvider
{
    string Name { get; }
    AICapabilities Capabilities { get; }
    
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);
    Task<AIResponse> GenerateWithContextAsync(AIRequest request, IConversationContext context, CancellationToken cancellationToken = default);
    Task<decimal> EstimateCostAsync(AIRequest request);
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
} 