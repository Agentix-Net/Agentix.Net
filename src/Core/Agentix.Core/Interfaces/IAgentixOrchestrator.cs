using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Simplified orchestrator interface focused on core message processing functionality
/// </summary>
public interface IAgentixOrchestrator
{
    /// <summary>
    /// Process an incoming message using the default AI provider
    /// </summary>
    Task<AIResponse> ProcessMessageAsync(IncomingMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Process an incoming message using a specific AI provider
    /// </summary>
    Task<AIResponse> ProcessMessageAsync(IncomingMessage message, string? providerName = null, CancellationToken cancellationToken = default);
} 