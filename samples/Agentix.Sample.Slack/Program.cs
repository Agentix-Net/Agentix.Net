using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Slack.Extensions;
using Agentix.Channels.Slack;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Sample.Slack;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("🚀 Starting Agentix Slack Sample...");
        
        // Get required configuration from environment variables or command line
        var claudeApiKey = GetConfigValue("CLAUDE_API_KEY", args, "--claude-key");
        var slackBotToken = GetConfigValue("SLACK_BOT_TOKEN", args, "--slack-token");
        var slackMode = GetConfigValue("SLACK_MODE", args, "--slack-mode")?.ToLower() ?? "webhook";
        
        // Mode-specific configuration
        var slackSigningSecret = GetConfigValue("SLACK_SIGNING_SECRET", args, "--slack-secret");
        var slackAppToken = GetConfigValue("SLACK_APP_TOKEN", args, "--slack-app-token");
        
        if (string.IsNullOrEmpty(claudeApiKey))
        {
            Console.WriteLine("❌ Claude API key is required. Set CLAUDE_API_KEY environment variable or use --claude-key");
            return;
        }
        
        if (string.IsNullOrEmpty(slackBotToken))
        {
            Console.WriteLine("❌ Slack bot token is required. Set SLACK_BOT_TOKEN environment variable or use --slack-token");
            return;
        }

        // Validate mode-specific requirements
        if (slackMode == "webhook" && string.IsNullOrEmpty(slackSigningSecret))
        {
            Console.WriteLine("❌ Slack signing secret is required for webhook mode. Set SLACK_SIGNING_SECRET environment variable or use --slack-secret");
            return;
        }

        if (slackMode == "socket" && string.IsNullOrEmpty(slackAppToken))
        {
            Console.WriteLine("❌ Slack app token is required for socket mode. Set SLACK_APP_TOKEN environment variable or use --slack-app-token");
            return;
        }

        // Create and configure the host builder
        var builder = Host.CreateDefaultBuilder(args);
        
        builder.ConfigureServices(services =>
        {
            // Configure Agentix with simplified fluent API
            services.AddAgentix(options =>
            {
                options.SystemPrompt = @"You are a helpful AI assistant integrated with Slack. You can:

- Answer questions and provide information
- Help with problem-solving and analysis
- Assist with coding and technical topics
- Provide explanations and clarifications
- Generate ideas and suggestions

When responding in Slack:
- Keep responses clear and conversational
- Use appropriate formatting (bold, italics, code blocks) when helpful
- Be concise but informative
- Adapt your tone to match the context of the conversation

You can respond to direct messages and mentions in channels. Feel free to use emojis to make conversations more engaging! 😊";
                options.EnableCostTracking = true;
            })
            .AddClaudeProvider(options =>
            {
                options.ApiKey = claudeApiKey;
                options.DefaultModel = "claude-3-haiku-20240307";
                options.Temperature = 0.7f;
                options.MaxTokens = 2000;
            })
            .AddSlackChannel(options =>
            {
                options.BotToken = slackBotToken;
                options.Mode = slackMode == "socket" ? SlackChannelMode.SocketMode : SlackChannelMode.Webhook;
                
                // Webhook-specific configuration
                if (options.Mode == SlackChannelMode.Webhook)
                {
                    options.SigningSecret = slackSigningSecret!;
                    options.WebhookPort = 3000;
                }
                
                // Socket Mode-specific configuration  
                if (options.Mode == SlackChannelMode.SocketMode)
                {
                    options.AppToken = slackAppToken!;
                    options.LogConnectionEvents = true; // Enable for debugging
                    options.AutoReconnect = true;
                }
                
                options.RespondToMentionsOnly = true;
                options.RespondToDirectMessages = true;
                options.UseThreading = true;
                options.ShowMetadata = false; // Set to true for debugging
                options.LogMessages = false; // Set to true for debugging
            });
        });

        Console.WriteLine("✅ Agentix Slack bot configured successfully!");
        Console.WriteLine($"🔧 Mode: {(slackMode == "socket" ? "Socket Mode" : "Webhook Mode")}");
        
        if (slackMode == "webhook")
        {
            Console.WriteLine("📡 Starting webhook server on port 3000...");
            Console.WriteLine("🔗 Make sure your Slack app is configured to send events to: http://your-domain:3000/slack/events");
        }
        else
        {
            Console.WriteLine("🔌 Connecting to Slack via WebSocket (Socket Mode)...");
            Console.WriteLine("💡 No public endpoint required - perfect for development!");
        }
        
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to stop the bot");

        // Build and run Agentix
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