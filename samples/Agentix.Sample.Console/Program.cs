using Agentix.Channels.Console;
using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Agentix.Core.Services;
using Agentix.Providers.Claude;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Sample.Console;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("🚀 AGENTIX v2 - Starting application...");

        try
        {
            // Check if Claude API key is provided
            System.Console.WriteLine("🔍 Checking API key...");
            var claudeApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY") ?? 
                              GetApiKeyFromArgs(args);

            System.Console.WriteLine($"🔍 API key length: {claudeApiKey?.Length ?? 0}");

            if (string.IsNullOrEmpty(claudeApiKey))
            {
                System.Console.WriteLine("❌ Claude API key not found!");
                System.Console.WriteLine("Please set the CLAUDE_API_KEY environment variable or pass it as argument:");
                System.Console.WriteLine("   dotnet run -- --api-key YOUR_API_KEY");
                System.Console.WriteLine("   or");
                System.Console.WriteLine("   set CLAUDE_API_KEY=YOUR_API_KEY");
                return;
            }

            System.Console.WriteLine("✅ API key found, creating services...");

            // Create service collection
            var services = new ServiceCollection();
            
            // Add logging
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning); // Reduce noise
            });

            // Add Agentix core services
            services.AddAgentixCore();

            // Build service provider
            var serviceProvider = services.BuildServiceProvider();
            
            // Configure system prompts
            serviceProvider.ConfigureSystemPrompts(prompts =>
            {
                // Set a default system prompt for the application
                prompts.SetDefaultSystemPrompt(@"You are an expert Architecture Decision Record (ADR) assistant. You help software teams document important architectural decisions using the ADR format.

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
- Focus on documenting the 'why' behind architectural decisions");
                
                // Set console-specific system prompt
                prompts.SetChannelSystemPrompt("console", @"You are an ADR (Architecture Decision Record) specialist. Be direct and practical in helping users create, review, and improve their architectural decision documentation. Keep responses focused and actionable while ensuring clarity in the decision-making process.");
            });
            
            System.Console.WriteLine("✅ Services created, setting up providers and channels...");

            // Get registries
            var providerRegistry = serviceProvider.GetRequiredService<IProviderRegistry>();
            var channelRegistry = serviceProvider.GetRequiredService<IChannelRegistry>();
            var orchestrator = serviceProvider.GetRequiredService<IAgentixOrchestrator>();

            // Create and register Claude provider
            var claudeOptions = new ClaudeOptions
            {
                ApiKey = claudeApiKey,
                DefaultModel = "claude-3-haiku-20240307",
                MaxTokens = 1000,
                Temperature = 0.7f
            };
            
            var providerLogger = serviceProvider.GetRequiredService<ILogger<ClaudeProvider>>();
            var claudeProvider = new ClaudeProvider(claudeOptions, providerLogger);
            providerRegistry.RegisterProvider(claudeProvider);
            
            System.Console.WriteLine("✅ Claude provider registered");

            // Create and register console channel
            var channelLogger = serviceProvider.GetRequiredService<ILogger<ConsoleChannelAdapter>>();
            var consoleChannel = new ConsoleChannelAdapter(orchestrator, channelLogger);
            channelRegistry.RegisterChannel(consoleChannel);
            
            System.Console.WriteLine("✅ Console channel registered");

            // Verify registration
            var allChannels = channelRegistry.GetAllChannels();
            System.Console.WriteLine($"🔍 Total channels registered: {allChannels.Count()}");
            
            foreach (var channel in allChannels)
            {
                System.Console.WriteLine($"   - {channel.Name} ({channel.ChannelType})");
            }

            System.Console.WriteLine();
            System.Console.WriteLine("📝 System Prompt Feature Configured:");
            System.Console.WriteLine("   ✅ Default system prompt set for AI assistant behavior");
            System.Console.WriteLine("   ✅ Console-specific system prompt configured");
            System.Console.WriteLine("   The AI will now respond with the configured personality and context.");
            System.Console.WriteLine();

            // Get and start the console channel
            var foundChannel = channelRegistry.GetChannel("console");
            if (foundChannel is ConsoleChannelAdapter consoleAdapter)
            {
                System.Console.WriteLine("✅ Console channel found, starting...");
                await consoleAdapter.StartAsync();
                
                // Wait for the console channel to stop (when user types /quit)
                while (consoleAdapter.IsRunning)
                {
                    await Task.Delay(1000);
                }
                
                System.Console.WriteLine("👋 Console channel stopped");
            }
            else
            {
                System.Console.WriteLine("❌ Console channel still not found!");
            }
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Application error: {ex.GetType().Name}: {ex.Message}");
            if (ex.InnerException != null)
            {
                System.Console.WriteLine($"   Inner exception: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            System.Console.WriteLine($"   Stack trace: {ex.StackTrace}");
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }
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