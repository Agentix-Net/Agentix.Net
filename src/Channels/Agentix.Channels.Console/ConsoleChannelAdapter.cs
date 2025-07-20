using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Microsoft.Extensions.Logging;

namespace Agentix.Channels.Console;

public class ConsoleChannelAdapter : IChannelAdapter
{
    private readonly IAgentixOrchestrator _orchestrator;
    private readonly ILogger<ConsoleChannelAdapter> _logger;
    private readonly ConsoleChannelOptions _options;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runningTask;

    public string Name => "console";
    public string ChannelType => "console";
    public bool IsRunning { get; private set; }

    // Console has basic capabilities
    public bool SupportsRichContent => false;
    public bool SupportsFileUploads => false;
    public bool SupportsInteractiveElements => false;

    public ConsoleChannelAdapter(
        IAgentixOrchestrator orchestrator, 
        ILogger<ConsoleChannelAdapter> logger,
        ConsoleChannelOptions? options = null)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _options = options ?? new ConsoleChannelOptions();
    }

    public Task<bool> CanHandleAsync(IncomingMessage message)
    {
        // Console channel can handle any text-based message
        return Task.FromResult(message.Channel == ChannelType || message.Channel == "console");
    }

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

    public Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default)
    {
        if (!response.Success)
        {
            WriteToConsole($"❌ Error: {response.ErrorMessage}", ConsoleColor.Red);
        }
        else
        {
            WriteToConsole($"🤖 {response.Content}", ConsoleColor.Green);
            
            if (_options.ShowMetadata && response.Usage.TotalTokens > 0)
            {
                WriteToConsole($"   📊 Tokens: {response.Usage.InputTokens}/{response.Usage.OutputTokens} | " +
                             $"Cost: ${response.EstimatedCost:F4} | " +
                             $"Time: {response.ResponseTime.TotalMilliseconds:F0}ms | " +
                             $"Provider: {response.ProviderId}", 
                             ConsoleColor.DarkGray);
            }
        }

        return Task.CompletedTask;
    }

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

        _logger.LogInformation("Console channel started");
    }

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
                    Content = input,
                    UserId = Environment.UserName,
                    UserName = Environment.UserName,
                    ChannelId = "console",
                    ChannelName = "Console",
                    Channel = ChannelType,
                    Type = MessageType.Text
                };

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

                System.Console.WriteLine(); // Add spacing
            }
        }
        catch (OperationCanceledException)
        {
            // Normal cancellation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in console loop");
        }
    }



    private void ShowWelcomeMessage()
    {
        System.Console.Clear();
        WriteToConsole("🚀 Welcome to Agentix Console!", ConsoleColor.Cyan);
        WriteToConsole("Type your messages to chat with AI. Commands:", ConsoleColor.Gray);
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
        WriteToConsole($"{Environment.UserName}> ", ConsoleColor.Yellow, false);
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

public class ConsoleChannelOptions
{
    public bool ShowMetadata { get; set; } = true;
    public string Prompt { get; set; } = "> ";
    public bool ClearOnStart { get; set; } = true;
} 