using Agentix.Core.Extensions;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Agentix.Context.InMemory.Extensions;
using Agentix.Rag.GitHub.Extensions;
using Agentix.Rag.InMemory.Extensions;
using Agentix.Rag.Embeddings.Local.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Agentix.Sample.RAG.Console;

class Program
{
    static async Task Main(string[] args)
    {
        System.Console.WriteLine("🚀 Starting Agentix RAG Console Sample...");
        System.Console.WriteLine("This sample demonstrates GitHub repository search integration with AI chat.");
        System.Console.WriteLine();

        // Load configuration from multiple sources
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build();

        // Get required API keys with fallback logic
        var claudeApiKey = GetRequiredConfig(configuration, "Claude:ApiKey", "CLAUDE_API_KEY", args, "--claude-key");
        var githubToken = GetRequiredConfig(configuration, "GitHub:Token", "GITHUB_TOKEN", args, "--github-token");

        // Check for missing keys
        var missingKeys = new List<string>();
        if (string.IsNullOrEmpty(claudeApiKey)) missingKeys.Add("Claude API Key");
        if (string.IsNullOrEmpty(githubToken)) missingKeys.Add("GitHub Token");

        if (missingKeys.Any())
        {
            ShowSetupInstructions(missingKeys.ToArray());
            return;
        }

        // Get repositories from config or command line
        var repositories = GetRepositories(configuration, args);
        if (!repositories.Any())
        {
            System.Console.WriteLine("❌ No repositories configured. Add repositories to appsettings.json or use --repo arguments.");
            System.Console.WriteLine("Example: --repo https://github.com/microsoft/dotnet --repo https://github.com/aspnet/core");
            System.Console.WriteLine();
            System.Console.WriteLine("💡 Note: Local embeddings will download models automatically (~25MB) on first use.");
            System.Console.WriteLine();
            ShowSetupInstructions();
            return;
        }

        System.Console.WriteLine($"📚 Configuring RAG for {repositories.Length} repositories:");
        foreach (var repo in repositories)
        {
            System.Console.WriteLine($"   • {repo}");
        }
        System.Console.WriteLine();

        try
        {
            // Create and configure the host
            var builder = Host.CreateDefaultBuilder(args);
            
            builder.ConfigureServices(services =>
            {
                // Configure Agentix with RAG
                services.AddAgentix(options =>
                {
                    options.SystemPrompt = @"You are an AI assistant with access to GitHub repositories through search. You can:

🔍 Search code repositories for implementations, examples, and documentation
📝 Find relevant code snippets and explain how they work  
🏗️ Help understand architecture and design patterns
🐛 Look up error handling and debugging approaches
📖 Find documentation and README files

When users ask about code, implementation, or 'how do we...' questions, use the github_search tool to find relevant information from the configured repositories.

Always provide:
- Direct quotes from relevant code/docs with GitHub file links when available
- Clear explanations of what the code does
- Context about where the code fits in the larger system
- Specific examples with file paths and line references when possible

Be helpful and thorough in explaining the code you find.";

                    options.EnableCostTracking = true;
                })
                .AddInMemoryContext()
                .AddClaudeProvider(options =>
                {
                    options.ApiKey = claudeApiKey;
                    options.DefaultModel = "claude-3-haiku-20240307";
                    options.Temperature = 0.7f;
                    options.MaxTokens = 4000;
                })
                .AddConsoleChannel(options =>
                {
                    options.WelcomeMessage = GetWelcomeMessage(repositories);
                    options.ShowMetadata = false;
                })
                .AddLocalEmbeddings() // Local ONNX embedding provider - zero cost!
                .AddInMemoryVectorStore() // Vector store implementation
                .AddGitHubRAG(options =>
                {
                    options.AccessToken = githubToken;
                    options.Repositories = repositories;
                }); // GitHub search tool is automatically registered with AddGitHubRAG()
            });

            System.Console.WriteLine("✅ Agentix RAG Console configured successfully!");
            System.Console.WriteLine("🤖 Using local embeddings (no external API costs!)");
            System.Console.WriteLine("🔄 Indexing repositories in the background...");
            System.Console.WriteLine("💬 You can start chatting while indexing continues.");
            System.Console.WriteLine();
            System.Console.WriteLine("Try asking:");
            System.Console.WriteLine("  • 'How is authentication implemented?'");
            System.Console.WriteLine("  • 'Show me error handling patterns'");
            System.Console.WriteLine("  • 'Find examples of API controllers'");
            System.Console.WriteLine("  • 'What testing frameworks are used?'");
            System.Console.WriteLine("  • 'How is dependency injection configured?'");
            System.Console.WriteLine();
            System.Console.WriteLine("Press Ctrl+C to stop");
            System.Console.WriteLine();

            // Build and run Agentix
            await builder.BuildAndRunAgentixAsync();
        }
        catch (Exception ex)
        {
            System.Console.WriteLine($"❌ Error starting application: {ex.Message}");
            System.Console.WriteLine();
            
            if (ex.Message.Contains("GitHub"))
            {
                System.Console.WriteLine("💡 This might be a GitHub access issue. Make sure:");
                System.Console.WriteLine("   • Your GitHub token is valid");
                System.Console.WriteLine("   • You have access to the repositories");
                System.Console.WriteLine("   • For private repos, ensure token has 'repo' scope");
            }
            else if (ex.Message.Contains("model") || ex.Message.Contains("download"))
            {
                System.Console.WriteLine("💡 This might be a model download issue. Make sure:");
                System.Console.WriteLine("   • You have an internet connection for initial model download");
                System.Console.WriteLine("   • You have sufficient disk space (~25MB for the embedding model)");
                System.Console.WriteLine("   • The model cache directory is writable");
            }
            
            Environment.Exit(1);
        }
    }

    static string GetRequiredConfig(IConfiguration config, string configKey, string envVar, string[] args, string argName)
    {
        // Try configuration file first
        var value = config[configKey];
        if (!string.IsNullOrEmpty(value)) return value;

        // Try environment variable
        value = Environment.GetEnvironmentVariable(envVar);
        if (!string.IsNullOrEmpty(value)) return value;

        // Try command line
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == argName) return args[i + 1];
        }

        return string.Empty;
    }

    static string[] GetRepositories(IConfiguration config, string[] args)
    {
        var repositories = new List<string>();

        // From configuration
        var configRepos = config.GetSection("GitHub:Repositories").Get<string[]>();
        if (configRepos?.Any() == true)
        {
            repositories.AddRange(configRepos);
        }

        // From command line --repo arguments
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (args[i] == "--repo")
            {
                repositories.Add(args[i + 1]);
            }
        }

        return repositories.Distinct().ToArray();
    }

    static string GetWelcomeMessage(string[] repositories)
    {
        var repoNames = repositories.Select(r => 
        {
            try
            {
                var uri = new Uri(r);
                return uri.AbsolutePath.Trim('/');
            }
            catch
            {
                return r;
            }
        }).ToArray();
        
        return $@"🤖 AI Assistant with GitHub Repository Search

📚 Configured Repositories: {string.Join(", ", repoNames)}

I can help you search and understand your codebase. Try asking:
• 'How do we handle authentication?'
• 'Show me API error handling patterns'  
• 'Find examples of unit tests'
• 'What's our logging strategy?'
• 'How is the database configured?'

Let's explore your code together! 🚀

Note: Repository indexing happens in the background. Initial searches may take a moment while content is being processed.";
    }

    static void ShowSetupInstructions(string[]? missingKeys = null)
    {
        System.Console.WriteLine("🔧 Setup Required");
        System.Console.WriteLine();

        if (missingKeys?.Any() == true)
        {
            System.Console.WriteLine($"❌ Missing: {string.Join(", ", missingKeys)}");
            System.Console.WriteLine();
        }

        System.Console.WriteLine("This sample needs two API keys:");
        System.Console.WriteLine();
        
        System.Console.WriteLine("1. 🤖 Claude API Key");
        System.Console.WriteLine("   Get from: https://console.anthropic.com/");
        System.Console.WriteLine("   Used for: AI conversations and code understanding");
        System.Console.WriteLine();
        
        System.Console.WriteLine("2. 🐙 GitHub Token");
        System.Console.WriteLine("   Get from: https://github.com/settings/tokens");
        System.Console.WriteLine("   Used for: Accessing repository content");
        System.Console.WriteLine("   Scopes needed: 'repo' (for private repos), 'public_repo' (for public repos)");
        System.Console.WriteLine();
        
        System.Console.WriteLine("✨ Local Embeddings (No API Key Needed!)");
        System.Console.WriteLine("   Uses: Local ONNX models for semantic search");
        System.Console.WriteLine("   Cost: Zero ongoing costs - completely free!");
        System.Console.WriteLine("   Note: ~25MB model download on first use");
        System.Console.WriteLine();

        System.Console.WriteLine("📁 Configuration options:");
        System.Console.WriteLine();
        
        System.Console.WriteLine("Option A: Configuration file (appsettings.json):");
        System.Console.WriteLine(@"{
  ""Claude"": {
    ""ApiKey"": ""your-claude-api-key""
  },
  ""GitHub"": {
    ""Token"": ""ghp_your-github-token"",
    ""Repositories"": [
      ""https://github.com/microsoft/dotnet"",
      ""https://github.com/aspnet/core""
    ]
  }
}");
        System.Console.WriteLine();
        
        System.Console.WriteLine("Option B: Environment variables:");
        System.Console.WriteLine("set CLAUDE_API_KEY=your-claude-api-key");
        System.Console.WriteLine("set GITHUB_TOKEN=ghp_your-github-token");
        System.Console.WriteLine();
        
        System.Console.WriteLine("Option C: Command line:");
        System.Console.WriteLine("dotnet run --claude-key your-key --github-token ghp_token --repo https://github.com/org/repo");
        System.Console.WriteLine();
        
        System.Console.WriteLine("💡 For more details, see README.md");
    }
} 