using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Interface that all AI providers must implement to integrate with the Agentix framework.
/// Providers are responsible for communicating with AI services and generating responses.
/// </summary>
public interface IAIProvider
{
    /// <summary>
    /// Gets the unique name identifier of this AI provider.
    /// </summary>
    /// <value>A string that uniquely identifies this provider (e.g., "claude", "openai").</value>
    string Name { get; }
    
    /// <summary>
    /// Gets the capabilities supported by this AI provider.
    /// </summary>
    /// <value>An <see cref="AICapabilities"/> object describing what features this provider supports.</value>
    AICapabilities Capabilities { get; }
    
    /// <summary>
    /// Generates an AI response for the given request without conversation context.
    /// </summary>
    /// <param name="request">The AI request containing the message and configuration.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AI response.</returns>
    Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Generates an AI response for the given request with conversation context.
    /// This method allows the provider to access message history and contextual data.
    /// </summary>
    /// <param name="request">The AI request containing the message and configuration.</param>
    /// <param name="context">The conversation context containing message history and data.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the AI response.</returns>
    Task<AIResponse> GenerateWithContextAsync(AIRequest request, IConversationContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Estimates the cost for processing the given AI request.
    /// </summary>
    /// <param name="request">The AI request to estimate cost for.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the estimated cost in USD.</returns>
    Task<decimal> EstimateCostAsync(AIRequest request);
    
    /// <summary>
    /// Performs a health check to verify the provider is working correctly.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the provider is healthy, false otherwise.</returns>
    Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default);
} 