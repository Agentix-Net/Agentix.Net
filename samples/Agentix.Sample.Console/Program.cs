using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
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

        // Create and configure the host builder
        var builder = Host.CreateDefaultBuilder(args);
        
        builder.ConfigureServices(services =>
        {
            // Configure Agentix with simplified fluent API
            services.AddAgentix(options =>
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
            .AddConsoleChannel(options =>
            {
                options.WelcomeMessage = "✅ You're talking to an ADR specialist.\n💡 Try asking: 'Help me create an ADR for choosing a database'";
                options.ShowMetadata = true;
            });
        });

        System.Console.WriteLine("✅ Agentix Console bot configured successfully!");
        System.Console.WriteLine("💬 Start typing to chat with the ADR assistant...");
        System.Console.WriteLine("   The AI will now respond with the configured personality and context.");
        System.Console.WriteLine();
        System.Console.WriteLine("Press Ctrl+C to stop the bot");

        // Build and run Agentix - simplified startup!
        await builder.BuildAndRunAgentixAsync();
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