using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Channels.Console;

/// <summary>
/// Console channel adapter that provides an interactive command-line interface for AI conversations.
/// Enables direct terminal interaction with AI agents through a text-based chat interface.
/// </summary>
/// <remarks>
/// This channel adapter is ideal for development, testing, and building CLI applications.
/// It supports built-in commands like /help and /quit, displays optional metadata
/// such as token usage and costs, and handles graceful shutdown.
/// </remarks>
public sealed class ConsoleChannelAdapter : IChannelAdapter
{
    private readonly IAgentixOrchestrator _orchestrator;
    private readonly ILogger<ConsoleChannelAdapter> _logger;
    private readonly ConsoleChannelOptions _options;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runningTask;

    /// <summary>
    /// Gets the unique name identifier for this channel adapter.
    /// </summary>
    /// <value>Always returns "console".</value>
    public string Name => "console";
    
    /// <summary>
    /// Gets the type of channel this adapter handles.
    /// </summary>
    /// <value>Always returns "console".</value>
    public string ChannelType => "console";
    
    /// <summary>
    /// Gets a value indicating whether this channel is currently running and accepting input.
    /// </summary>
    /// <value>True if the console loop is active and processing user input; otherwise, false.</value>
    public bool IsRunning { get; private set; }

    /// <summary>
    /// Gets a value indicating whether this channel supports rich content such as formatting or images.
    /// </summary>
    /// <value>Always returns false. The console channel only supports plain text.</value>
    public bool SupportsRichContent => false;
    
    /// <summary>
    /// Gets a value indicating whether this channel supports file uploads from users.
    /// </summary>
    /// <value>Always returns false. The console channel does not support file uploads.</value>
    public bool SupportsFileUploads => false;
    
    /// <summary>
    /// Gets a value indicating whether this channel supports interactive elements such as buttons.
    /// </summary>
    /// <value>Always returns false. The console channel only supports text-based interaction.</value>
    public bool SupportsInteractiveElements => false;

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleChannelAdapter"/> class.
    /// </summary>
    /// <param name="orchestrator">The Agentix orchestrator for processing AI requests.</param>
    /// <param name="logger">Logger instance for diagnostic and error information.</param>
    /// <param name="options">Configuration options for the console channel. If null, default options are used.</param>
    public ConsoleChannelAdapter(
        IAgentixOrchestrator orchestrator, 
        ILogger<ConsoleChannelAdapter> logger,
        ConsoleChannelOptions? options = null)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _options = options ?? new ConsoleChannelOptions();
    }

    /// <summary>
    /// Determines whether this channel adapter can handle the specified message.
    /// </summary>
    /// <param name="message">The incoming message to evaluate.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is true if the message is from the console channel; otherwise, false.</returns>
    public Task<bool> CanHandleAsync(IncomingMessage message)
    {
        // Console channel can handle any text-based message
        return Task.FromResult(message.Channel == ChannelType || message.Channel == "console");
    }

    /// <summary>
    /// Processes an incoming message and coordinates with the AI provider to generate a response.
    /// </summary>
    /// <param name="message">The incoming message to process.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the channel response.</returns>
    /// <remarks>
    /// This method handles built-in commands like /help and /quit, and forwards other messages
    /// to the AI orchestrator for processing. Built-in commands are processed locally without
    /// involving the AI provider.
    /// </remarks>
    public async Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Processing console message: {Content}", message.Content);

            // Check for special commands
            if (message.Content.ToLowerInvariant().Trim() == "/quit" || 
                message.Content.ToLowerInvariant().Trim() == "/exit")
            {
                _logger.LogInformation("Received quit command, stopping console channel");
                _ = Task.Run(() => StopAsync(CancellationToken.None));
                
                return new ChannelResponse
                {
                    Success = true,
                    AIResponse = new AIResponse
                    {
                        Content = "Goodbye! 👋",
                        Success = true,
                        ProviderId = "system"
                    }
                };
            }

            if (message.Content.ToLowerInvariant().Trim() == "/help")
            {
                return new ChannelResponse
                {
                    Success = true,
                    AIResponse = new AIResponse
                    {
                        Content = GetHelpMessage(),
                        Success = true,
                        ProviderId = "system"
                    }
                };
            }

            // Process with AI
            var aiResponse = await _orchestrator.ProcessMessageAsync(message, cancellationToken);

            return new ChannelResponse
            {
                Success = true,
                AIResponse = aiResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing console message");
            
            return new ChannelResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    /// <summary>
    /// Sends an AI response to the console output.
    /// </summary>
    /// <param name="response">The AI response to display.</param>
    /// <param name="context">The message context containing routing and metadata information.</param>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous send operation.</returns>
    /// <remarks>
    /// This method formats and displays the AI response in the console with appropriate colors.
    /// If <see cref="ConsoleChannelOptions.ShowMetadata"/> is enabled, it also displays
    /// token usage and cost information.
    /// </remarks>
    public Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default)
    {
        if (!response.Success)
        {
            WriteToConsole($"❌ Error: {response.ErrorMessage}", ConsoleColor.Red);
        }
        else
        {
            WriteToConsole($"Assistant: {response.Content}", ConsoleColor.Green);
            
            if (_options.ShowMetadata && response.Usage.TotalTokens > 0)
            {
                WriteToConsole($"📊 Tokens used: {response.Usage.InputTokens} in, {response.Usage.OutputTokens} out (${response.EstimatedCost:F4})", 
                             ConsoleColor.DarkGray);
            }
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts the console channel and begins the interactive input loop.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous start operation.</returns>
    /// <remarks>
    /// This method displays a welcome message, starts the console input loop in a background task,
    /// and begins accepting user input. The console remains active until <see cref="StopAsync"/> is called
    /// or the user enters a quit command.
    /// </remarks>
    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            _logger.LogWarning("Console channel is already running");
            return;
        }

        _logger.LogInformation("Starting console channel...");

        _cancellationTokenSource = new CancellationTokenSource();
        IsRunning = true;

        ShowWelcomeMessage();

        // Start the console input loop
        _runningTask = Task.Run(async () => await RunConsoleLoopAsync(_cancellationTokenSource.Token), cancellationToken);

        _logger.LogInformation("Console channel started and ready for interaction");
    }

    /// <summary>
    /// Stops the console channel and terminates the interactive input loop.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
    /// <returns>A task that represents the asynchronous stop operation.</returns>
    /// <remarks>
    /// This method gracefully shuts down the console input loop, displays a goodbye message,
    /// and cleans up resources. Any pending user input will be cancelled.
    /// </remarks>
    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            _logger.LogWarning("Console channel is not running");
            return;
        }

        _logger.LogInformation("Stopping console channel...");
        
        IsRunning = false;
        _cancellationTokenSource?.Cancel();

        if (_runningTask != null)
        {
            try
            {
                await _runningTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _runningTask = null;

        WriteToConsole("👋 Goodbye!", ConsoleColor.Cyan);
        _logger.LogInformation("Console channel stopped");
    }

    private async Task RunConsoleLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsRunning)
            {
                WritePrompt();
                
                // Read user input
                var input = await ReadLineAsync(cancellationToken);
                
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                // Create incoming message
                var message = new IncomingMessage
                {
                    Content = input.Trim(),
                    UserId = _options.DefaultUserId ?? Environment.UserName,
                    UserName = _options.DefaultUserName ?? Environment.UserName,
                    ChannelId = "console",
                    ChannelName = "Console",
                    Channel = ChannelType,
                    Type = MessageType.Text
                };

                try
                {
                    // Process the message
                    var response = await ProcessAsync(message, cancellationToken);
                    
                    // Send the response
                    if (response.AIResponse != null)
                    {
                        var context = new MessageContext
                        {
                            ChannelId = message.ChannelId,
                            UserId = message.UserId,
                            Channel = message.Channel,
                            OriginalMessage = message
                        };

                        await SendResponseAsync(response.AIResponse, context, cancellationToken);
                    }
                    else if (!response.Success)
                    {
                        WriteToConsole($"❌ Error: {response.ErrorMessage}", ConsoleColor.Red);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing message: {Message}", input);
                    WriteToConsole($"❌ Unexpected error: {ex.Message}", ConsoleColor.Red);
                }

                System.Console.WriteLine(); // Add spacing between interactions
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in console loop");
            WriteToConsole($"❌ Console loop error: {ex.Message}", ConsoleColor.Red);
        }
    }

    private void ShowWelcomeMessage()
    {
        if (_options.ClearOnStart)
        {
            System.Console.Clear();
        }
        
        WriteToConsole("🚀 Agentix Console ready!", ConsoleColor.Cyan);
        
        if (!string.IsNullOrEmpty(_options.WelcomeMessage))
        {
            WriteToConsole(_options.WelcomeMessage, ConsoleColor.Gray);
        }
        
        WriteToConsole("💡 Type your messages or commands:", ConsoleColor.Gray);
        WriteToConsole("  /help - Show help", ConsoleColor.Gray);
        WriteToConsole("  /quit - Exit the application", ConsoleColor.Gray);
        System.Console.WriteLine();
    }

    private string GetHelpMessage()
    {
        return @"📖 Agentix Console Help:

Commands:
  /help - Show this help message
  /quit - Exit the application
  /exit - Exit the application

Just type your message and press Enter to chat with the AI!
The AI can help you with questions, coding, analysis, and more.

The AI's behavior is configured by the application developer through
system prompts that define its personality and expertise areas.";
    }

    private void WritePrompt()
    {
        WriteToConsole($"You: ", ConsoleColor.Yellow, false);
    }

    private void WriteToConsole(string message, ConsoleColor color = ConsoleColor.White, bool newLine = true)
    {
        var originalColor = System.Console.ForegroundColor;
        System.Console.ForegroundColor = color;
        
        if (newLine)
        {
            System.Console.WriteLine(message);
        }
        else
        {
            System.Console.Write(message);
        }
        
        System.Console.ForegroundColor = originalColor;
    }

    private async Task<string> ReadLineAsync(CancellationToken cancellationToken)
    {
        // Simple implementation - in a real scenario, you might want to use
        // a more sophisticated async console reading mechanism
        return await Task.Run(() =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (System.Console.KeyAvailable)
                {
                    return System.Console.ReadLine() ?? string.Empty;
                }
                Thread.Sleep(50);
            }
            return string.Empty;
        }, cancellationToken);
    }
}

/// <summary>
/// Configuration options for the console channel adapter.
/// Contains settings for display behavior, user interaction, and session management.
/// </summary>
public sealed class ConsoleChannelOptions
{
    /// <summary>
    /// Gets or sets a value indicating whether to show metadata such as token usage and costs after each AI response.
    /// </summary>
    /// <value>True to display token counts and cost information; otherwise, false. Defaults to true.</value>
    /// <remarks>
    /// When enabled, the console will show information like "📊 Tokens used: 45 in, 128 out ($0.0023)"
    /// after each AI response to help track usage and costs.
    /// </remarks>
    public bool ShowMetadata { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the prompt string displayed before user input in the console.
    /// </summary>
    /// <value>The prompt string. Defaults to "> ".</value>
    /// <remarks>
    /// This is the text shown to indicate where the user should type their input.
    /// Common examples include "> ", "$ ", or "Assistant: ".
    /// </remarks>
    public string Prompt { get; set; } = "> ";
    
    /// <summary>
    /// Gets or sets a value indicating whether to clear the console screen when the channel starts.
    /// </summary>
    /// <value>True to clear the console on startup; otherwise, false. Defaults to true.</value>
    /// <remarks>
    /// Clearing the console provides a clean slate for the AI conversation.
    /// Disable this if you want to preserve existing console content.
    /// </remarks>
    public bool ClearOnStart { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a custom welcome message to display when the console channel starts.
    /// </summary>
    /// <value>The welcome message text, or null to use the default message.</value>
    /// <remarks>
    /// If null, a default welcome message will be shown. Set to an empty string
    /// to suppress the welcome message entirely. Supports multi-line text and emoji.
    /// </remarks>
    public string? WelcomeMessage { get; set; }
    
    /// <summary>
    /// Gets or sets the default user ID to associate with console messages.
    /// </summary>
    /// <value>The user ID string, or null to use the current system user.</value>
    /// <remarks>
    /// This ID is used for conversation context and tracking. If null,
    /// the current environment username is used automatically.
    /// </remarks>
    public string? DefaultUserId { get; set; }
    
    /// <summary>
    /// Gets or sets the default user display name for console messages.
    /// </summary>
    /// <value>The user display name, or null to use the current system user.</value>
    /// <remarks>
    /// This name is used for display purposes and logging. If null,
    /// the current environment username is used automatically.
    /// </remarks>
    public string? DefaultUserName { get; set; }
} 