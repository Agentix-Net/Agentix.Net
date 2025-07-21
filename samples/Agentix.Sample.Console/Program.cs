using System;
using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Agentix.Context.InMemory.Extensions;
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
        var claudeApiKey = GetConfigValue("CLAUDE_API_KEY", args, "--claude-key");
        
        if (string.IsNullOrEmpty(claudeApiKey))
        {
            System.Console.Write("Please enter your Claude API key: ");
            claudeApiKey = System.Console.ReadLine();
            if (string.IsNullOrEmpty(claudeApiKey))
            {
                System.Console.WriteLine("❌ Claude API key is required. Set CLAUDE_API_KEY environment variable or use --claude-key");
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
                options.SystemPrompt = @"You are a helpful AI assistant integrated with a console application. You can:

- Answer questions and provide information
- Help with problem-solving and analysis
- Assist with coding and technical topics
- Provide explanations and clarifications
- Generate ideas and suggestions

When responding in the console:
- Keep responses clear and well-formatted
- Use appropriate structure for readability
- Be concise but informative
- Provide helpful examples when relevant

Feel free to use emojis to make conversations more engaging! 😊";
                options.EnableCostTracking = true;
            })
            .AddInMemoryContext()
            .AddClaudeProvider(options =>
            {
                options.ApiKey = claudeApiKey;
                options.DefaultModel = "claude-3-haiku-20240307";
                options.Temperature = 0.7f;
                options.MaxTokens = 2000;
            })
            .AddConsoleChannel(options =>
            {
                options.WelcomeMessage = "✅ Console AI Assistant is ready!\n💡 Try asking: 'How can you help me?' or 'Explain dependency injection in .NET'";
                options.ShowMetadata = false; // Set to true for debugging
            });
        });

        System.Console.WriteLine("✅ Agentix Console bot configured successfully!");
        System.Console.WriteLine("💬 Start typing to chat with the AI assistant...");
        System.Console.WriteLine();
        System.Console.WriteLine("Press Ctrl+C to stop the bot");

        // Build and run Agentix - simplified startup!
        await builder.BuildAndRunAgentixAsync();
    }

    static string? GetConfigValue(string envVar, string[] args, string argName)
    {
        // First try environment variable
        var value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(value))
        {
            return value;
        }

        // Then try command line arguments
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName)
            {
                return args[i + 1];
            }
        }

        return null;
    }
}