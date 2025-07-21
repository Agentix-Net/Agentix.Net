using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Interface that all communication channel adapters must implement to integrate with the Agentix framework.
/// Channels are responsible for receiving messages from users and sending responses back through specific communication platforms.
/// </summary>
public interface IChannelAdapter
{
    /// <summary>
    /// Gets the unique name identifier of this channel adapter.
    /// </summary>
    /// <value>A string that uniquely identifies this channel (e.g., "console", "slack", "teams").</value>
    string Name { get; }
    
    /// <summary>
    /// Gets the type of channel this adapter handles.
    /// </summary>
    /// <value>A string representing the channel type (e.g., "console", "slack", "teams").</value>
    string ChannelType { get; }
    
    /// <summary>
    /// Gets a value indicating whether this channel is currently running and accepting messages.
    /// </summary>
    /// <value>True if the channel is running; otherwise, false.</value>
    bool IsRunning { get; }
    
    /// <summary>
    /// Starts the channel adapter and begins listening for incoming messages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    Task StartAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Stops the channel adapter and ceases listening for incoming messages.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    Task StopAsync(CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Determines whether this channel adapter can handle the specified message.
    /// </summary>
    /// <param name="message">The incoming message to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if this channel can handle the message; otherwise, false.</returns>
    Task<bool> CanHandleAsync(IncomingMessage message);
    
    /// <summary>
    /// Processes an incoming message and coordinates with the AI provider to generate a response.
    /// </summary>
    /// <param name="message">The incoming message to process.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the channel response.</returns>
    Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Sends an AI response back to the user through this channel.
    /// </summary>
    /// <param name="response">The AI response to send.</param>
    /// <param name="context">The message context containing routing and metadata information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a value indicating whether this channel supports rich content such as formatting, images, or interactive elements.
    /// </summary>
    /// <value>True if the channel supports rich content; otherwise, false.</value>
    bool SupportsRichContent { get; }
    
    /// <summary>
    /// Gets a value indicating whether this channel supports file uploads from users.
    /// </summary>
    /// <value>True if the channel supports file uploads; otherwise, false.</value>
    bool SupportsFileUploads { get; }
    
    /// <summary>
    /// Gets a value indicating whether this channel supports interactive elements such as buttons or menus.
    /// </summary>
    /// <value>True if the channel supports interactive elements; otherwise, false.</value>
    bool SupportsInteractiveElements { get; }
} 