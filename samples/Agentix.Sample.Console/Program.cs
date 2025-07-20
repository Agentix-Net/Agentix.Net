using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Sample.Console;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("🚀 Starting Agentix Console Sample...");
        
        // Get Claude API key from command line, environment, or user input
        var claudeApiKey = GetApiKeyFromArgs(args) ?? Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        if (string.IsNullOrEmpty(claudeApiKey))
        {
            System.Console.Write("Please enter your Claude API key: ");
            claudeApiKey = System.Console.ReadLine();
            if (string.IsNullOrEmpty(claudeApiKey))
            {
                System.Console.WriteLine("❌ Claude API key is required. Exiting...");
                return;
            }
        }

        // Create host builder
        var builder = Host.CreateDefaultBuilder(args);
        
        // Configure services with simplified system prompt configuration
        builder.ConfigureServices(services =>
        {
            services.AddAgentixCore(options =>
            {
                options.SystemPrompt = @"You are an expert Architecture Decision Record (ADR) assistant. You help software teams document important architectural decisions using the ADR format.

You are knowledgeable about:
- ADR templates and best practices (especially Michael Nygard's format)
- Software architecture patterns and trade-offs
- Decision documentation frameworks
- Stakeholder communication in technical decisions
- Architectural analysis and decision-making processes

When helping with ADRs, you:
- Guide users through the standard ADR structure (Title, Status, Context, Decision, Consequences)
- Ask clarifying questions to understand the architectural problem and constraints
- Suggest relevant options and trade-offs to consider
- Help write clear, concise, and well-structured ADRs
- Provide examples and templates when helpful
- Focus on documenting the 'why' behind architectural decisions

Be direct and practical in helping users create, review, and improve their architectural decision documentation. Keep responses focused and actionable while ensuring clarity in the decision-making process.";
                options.EnableCostTracking = true;
            })
            .AddClaudeProvider(options =>
            {
                options.ApiKey = claudeApiKey;
                options.DefaultModel = "claude-3-haiku-20240307";
                options.Temperature = 0.7f;
                options.MaxTokens = 1000;
            })
            .AddConsoleChannel();
        });

        // Build and run the application
        var app = builder.Build();

        // Get the orchestrator and start the application
        var orchestrator = app.Services.GetRequiredService<IAgentixOrchestrator>();
        await orchestrator.StartAsync();

        System.Console.WriteLine("✅ Agentix Console ready! You're talking to an ADR specialist.");
        System.Console.WriteLine("💡 Try asking: 'Help me create an ADR for choosing a database'");
        System.Console.WriteLine("📝 Type your messages or '/quit' to exit.");
        System.Console.WriteLine();

        // Main conversation loop
        while (true)
        {
            System.Console.Write("You: ");
            var input = System.Console.ReadLine();
            
            if (string.IsNullOrEmpty(input))
                continue;
                
            if (input.ToLowerInvariant() == "/quit")
                break;

            try
            {
                var message = new Agentix.Core.Models.IncomingMessage
                {
                    Content = input,
                    UserId = "console-user",
                    ChannelId = "console",
                    Channel = "console"
                };

                var response = await orchestrator.ProcessMessageAsync(message, CancellationToken.None);
                
                if (response.Success)
                {
                    System.Console.WriteLine($"Assistant: {response.Content}");
                    
                    if (response.Usage.TotalTokens > 0)
                    {
                        System.Console.WriteLine($"📊 Tokens used: {response.Usage.InputTokens} in, {response.Usage.OutputTokens} out (${response.EstimatedCost:F4})");
                    }
                }
                else
                {
                    System.Console.WriteLine($"❌ Error: {response.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Unexpected error: {ex.Message}");
            }
            
            System.Console.WriteLine();
        }

        System.Console.WriteLine("�� Goodbye!");
    }

    static string? GetApiKeyFromArgs(string[] args)
    {
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--api-key" || args[i] == "-k")
            {
                return args[i + 1];
            }
        }
        return null;
    }
}