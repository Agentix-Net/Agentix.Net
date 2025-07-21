using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Agentix.Providers.Claude.Extensions;
using Agentix.Channels.Console.Extensions;
using Agentix.Channels.Slack.Extensions;
using Agentix.Context.InMemory.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Agentix.Tests.Integration;

/// <summary>
/// Tests for service registration of real framework components
/// </summary>
public class ServiceRegistrationTests
{
    [Fact]
    public void AddClaudeProvider_WithValidConfiguration_RegistersProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test provider registration without context dependency
        services.AddAgentix()
            .AddClaudeProvider(options =>
            {
                options.ApiKey = "test-key-not-used";
                options.DefaultModel = "claude-3-haiku-20240307";
                options.Temperature = 0.7f;
                options.MaxTokens = 1000;
            });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var provider = serviceProvider.GetService<IAIProvider>();
        Assert.NotNull(provider);
        Assert.Equal("claude", provider.Name);
        Assert.NotNull(provider.Capabilities);
    }

    [Fact]
    public void AddConsoleChannel_WithValidConfiguration_RegistersChannel()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test channel registration without context dependency
        services.AddAgentix()
            .AddConsoleChannel(options =>
            {
                options.WelcomeMessage = "Test welcome";
                options.ShowMetadata = true;
            });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var channel = serviceProvider.GetService<IChannelAdapter>();
        Assert.NotNull(channel);
        Assert.Equal("console", channel.Name);
        Assert.Equal("console", channel.ChannelType);
    }

    [Fact]
    public void AddSlackChannel_WithValidConfiguration_RegistersChannel()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test channel registration without context dependency
        services.AddAgentix()
            .AddSlackChannel(options =>
            {
                options.BotToken = "test-token-not-used";
                options.SigningSecret = "test-secret-not-used";
                options.RespondToDirectMessages = true;
                options.RespondToMentionsOnly = false;
            });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var channel = serviceProvider.GetService<IChannelAdapter>();
        Assert.NotNull(channel);
        Assert.Equal("slack", channel.Name);
        Assert.Equal("slack", channel.ChannelType);
        Assert.True(channel.SupportsRichContent);
        Assert.True(channel.SupportsFileUploads);
        Assert.True(channel.SupportsInteractiveElements);
    }

    [Fact]
    public void AddInMemoryContext_RegistersContextStore()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - This test specifically tests context registration, so keep context
        services.AddAgentix()
            .AddInMemoryContext();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var contextStore = serviceProvider.GetService<IContextStore>();
        var contextResolver = serviceProvider.GetService<IContextResolver>();
        Assert.NotNull(contextStore);
        Assert.NotNull(contextResolver);
    }

    [Fact]
    public void CompleteFrameworkSetup_WithAllComponents_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test complete setup including context (stateful mode)
        services.AddAgentix(options =>
        {
            options.SystemPrompt = "Test system prompt";
            options.EnableCostTracking = true;
            options.MaxConcurrentRequests = 5;
        })
        .AddInMemoryContext()
        .AddClaudeProvider(options =>
        {
            options.ApiKey = "test-key";
            options.DefaultModel = "claude-3-haiku-20240307";
        })
        .AddConsoleChannel(options =>
        {
            options.WelcomeMessage = "Test welcome";
        });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - All core services should be registered
        Assert.NotNull(serviceProvider.GetService<IAgentixOrchestrator>());
        Assert.NotNull(serviceProvider.GetService<IContextResolver>());
        Assert.NotNull(serviceProvider.GetService<IContextStore>());
        Assert.NotNull(serviceProvider.GetService<IAIProvider>());
        Assert.NotNull(serviceProvider.GetService<IChannelAdapter>());
        
        // Assert - Configuration should be applied
        var options = serviceProvider.GetService<AgentixOptions>();
        Assert.NotNull(options);
        Assert.Equal("Test system prompt", options.SystemPrompt);
        Assert.True(options.EnableCostTracking);
        Assert.Equal(5, options.MaxConcurrentRequests);
    }

    [Fact]
    public void MultipleProviders_RegisteredCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test multiple provider registration without context dependency
        services.AddAgentix()
            .AddClaudeProvider(options =>
            {
                options.ApiKey = "claude-test-key";
                options.DefaultModel = "claude-3-haiku-20240307";
            });

        // Note: In a real scenario, we'd add multiple different providers
        // For now, we test that the registration pattern works
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var providers = serviceProvider.GetServices<IAIProvider>().ToList();
        Assert.Single(providers); // Only one provider registered in this test
        Assert.Equal("claude", providers.First().Name);
    }

    [Fact]
    public void MultipleChannels_RegisteredCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test multiple channel registration without context dependency
        services.AddAgentix()
            .AddConsoleChannel(options =>
            {
                options.WelcomeMessage = "Console welcome";
            })
            .AddSlackChannel(options =>
            {
                options.BotToken = "test-token";
                options.SigningSecret = "test-secret";
            });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var channels = serviceProvider.GetServices<IChannelAdapter>().ToList();
        Assert.Equal(2, channels.Count);
        
        var channelNames = channels.Select(c => c.Name).ToList();
        Assert.Contains("console", channelNames);
        Assert.Contains("slack", channelNames);
    }

    [Fact]
    public void ServiceRegistration_StatelessMode_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test that minimal stateless configuration works
        services.AddAgentix();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Core services should be registered even without context
        Assert.NotNull(serviceProvider.GetService<IAgentixOrchestrator>());
        Assert.NotNull(serviceProvider.GetService<AgentixOptions>());
        
        // Context services should NOT be registered in stateless mode
        Assert.Null(serviceProvider.GetService<IContextResolver>());
        Assert.Null(serviceProvider.GetService<IContextStore>());
    }

    [Fact]
    public void ServiceRegistration_StatefulMode_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test that context-enabled configuration works
        services.AddAgentix()
            .AddInMemoryContext();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - All services including context should be registered
        Assert.NotNull(serviceProvider.GetService<IAgentixOrchestrator>());
        Assert.NotNull(serviceProvider.GetService<IContextResolver>());
        Assert.NotNull(serviceProvider.GetService<IContextStore>());
        Assert.NotNull(serviceProvider.GetService<AgentixOptions>());
    }

    [Fact]
    public void CompleteFrameworkSetup_StatelessMode_RegistersCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test complete setup without context (stateless mode)
        services.AddAgentix(options =>
        {
            options.SystemPrompt = "Test system prompt";
            options.EnableCostTracking = true;
            options.MaxConcurrentRequests = 5;
        })
        .AddClaudeProvider(options =>
        {
            options.ApiKey = "test-key";
            options.DefaultModel = "claude-3-haiku-20240307";
        })
        .AddConsoleChannel(options =>
        {
            options.WelcomeMessage = "Test welcome";
        });
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Core services should be registered, context services should not
        Assert.NotNull(serviceProvider.GetService<IAgentixOrchestrator>());
        Assert.NotNull(serviceProvider.GetService<IAIProvider>());
        Assert.NotNull(serviceProvider.GetService<IChannelAdapter>());
        
        // Context services should NOT be available in stateless mode
        Assert.Null(serviceProvider.GetService<IContextResolver>());
        Assert.Null(serviceProvider.GetService<IContextStore>());
        
        // Assert - Configuration should be applied
        var options = serviceProvider.GetService<AgentixOptions>();
        Assert.NotNull(options);
        Assert.Equal("Test system prompt", options.SystemPrompt);
        Assert.True(options.EnableCostTracking);
        Assert.Equal(5, options.MaxConcurrentRequests);
    }

    [Fact]
    public void ProviderHealthCheck_WithoutExternalDependency_DoesNotThrow()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddAgentix()
            .AddClaudeProvider(options =>
            {
                options.ApiKey = "invalid-key-for-testing";
                options.DefaultModel = "claude-3-haiku-20240307";
            });
        
        var serviceProvider = services.BuildServiceProvider();
        var provider = serviceProvider.GetRequiredService<IAIProvider>();
        
        // Act & Assert - Should not throw during service registration
        // Note: We're not actually calling HealthCheckAsync() here because that would
        // require external dependencies. We're just validating that the service
        // can be instantiated and resolved correctly.
        Assert.NotNull(provider);
        Assert.Equal("claude", provider.Name);
    }
} 