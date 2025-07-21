using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Context.InMemory.Extensions;
using Agentix.Tests.Integration.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Agentix.Tests.Configuration;

/// <summary>
/// Tests for core framework configuration and service registration
/// </summary>
public class FrameworkConfigurationTests
{
    [Fact]
    public void AddAgentix_WithBasicConfiguration_RegistersRequiredServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test stateless mode (no context)
        services.AddAgentix();
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        Assert.NotNull(serviceProvider.GetService<IAgentixOrchestrator>());
        Assert.NotNull(serviceProvider.GetService<AgentixOptions>());
        
        // Context services should NOT be registered in stateless mode
        Assert.Null(serviceProvider.GetService<IContextResolver>());
        Assert.Null(serviceProvider.GetService<IContextStore>());
    }

    [Fact]
    public void AddAgentix_WithCustomOptions_ConfiguresOptionsCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        const string customPrompt = "Custom test prompt";
        const bool customCostTracking = false;
        const int customConcurrency = 5;
        
        // Act - Test without context to verify options configuration works independently
        services.AddAgentix(options =>
        {
            options.SystemPrompt = customPrompt;
            options.EnableCostTracking = customCostTracking;
            options.MaxConcurrentRequests = customConcurrency;
        });
        
        var serviceProvider = services.BuildServiceProvider();
        var agentixOptions = serviceProvider.GetRequiredService<AgentixOptions>();
        
        // Assert
        Assert.Equal(customPrompt, agentixOptions.SystemPrompt);
        Assert.Equal(customCostTracking, agentixOptions.EnableCostTracking);
        Assert.Equal(customConcurrency, agentixOptions.MaxConcurrentRequests);
    }

    [Fact]
    public void AddAgentix_WithProviderAndChannel_RegistersAllComponents()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test without context to verify provider/channel registration works independently
        services.AddAgentix()
            .AddProvider<MockAIProvider>()
            .AddChannel<MockChannelAdapter>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var orchestrator = serviceProvider.GetService<IAgentixOrchestrator>();
        var providers = serviceProvider.GetServices<IAIProvider>();
        var channels = serviceProvider.GetServices<IChannelAdapter>();
        
        Assert.NotNull(orchestrator);
        Assert.Single(providers);
        Assert.Single(channels);
        Assert.Equal("mock-ai", providers.First().Name);
        Assert.Equal("mock-channel", channels.First().Name);
    }

    [Fact]
    public void AddAgentix_WithMultipleProviders_RegistersAllProviders()
    {
        // Arrange
        var services = new ServiceCollection();
        var mockProvider1 = new MockAIProvider { MockResponse = "Provider 1" };
        var mockProvider2 = new MockAIProvider { MockResponse = "Provider 2" };
        
        // Override Name property for distinction
        var provider1 = new TestProvider("test-provider-1");
        var provider2 = new TestProvider("test-provider-2");
        
        // Act - Test without context to verify multiple provider registration works independently
        services.AddAgentix()
            .AddProvider(provider1)
            .AddProvider(provider2);
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var providers = serviceProvider.GetServices<IAIProvider>().ToList();
        Assert.Equal(2, providers.Count);
        Assert.Contains(providers, p => p.Name == "test-provider-1");
        Assert.Contains(providers, p => p.Name == "test-provider-2");
    }

    [Fact]
    public void AddAgentix_WithCustomContextResolver_RegistersCustomResolver()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - This test specifically tests context functionality, so keep context
        services.AddAgentix()
            .AddInMemoryContext()
            .WithContextResolver<TestContextResolver>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var contextResolver = serviceProvider.GetService<IContextResolver>();
        Assert.NotNull(contextResolver);
        Assert.IsType<TestContextResolver>(contextResolver);
    }

    [Fact]
    public void AgentixBuilder_AccessToServices_AllowsAdvancedConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test without context to verify builder pattern works independently
        var builder = services.AddAgentix();
        builder.Services.AddSingleton<ITestService, TestService>();
        
        var serviceProvider = services.BuildServiceProvider();
        
        // Assert
        var testService = serviceProvider.GetService<ITestService>();
        Assert.NotNull(testService);
        Assert.IsType<TestService>(testService);
    }

    [Fact]
    public void AddAgentix_WithoutContext_WorksCorrectly()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test that framework works correctly in stateless mode
        services.AddAgentix()
            .AddProvider<MockAIProvider>()
            .AddChannel<MockChannelAdapter>();

        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Should resolve without context
        var orchestrator = serviceProvider.GetRequiredService<IAgentixOrchestrator>();
        Assert.NotNull(orchestrator);
        
        // Context services should be null in stateless mode
        Assert.Null(serviceProvider.GetService<IContextStore>());
        Assert.Null(serviceProvider.GetService<IContextResolver>());
    }

    [Fact]
    public void AddAgentix_WithContext_RegistersContextServices()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act - Test that context services are properly registered when explicitly added
        services.AddAgentix()
            .AddInMemoryContext()
            .AddProvider<MockAIProvider>()
            .AddChannel<MockChannelAdapter>();

        var serviceProvider = services.BuildServiceProvider();
        
        // Assert - Both orchestrator and context services should be available
        var orchestrator = serviceProvider.GetRequiredService<IAgentixOrchestrator>();
        var contextStore = serviceProvider.GetRequiredService<IContextStore>();
        var contextResolver = serviceProvider.GetRequiredService<IContextResolver>();
        
        Assert.NotNull(orchestrator);
        Assert.NotNull(contextStore);
        Assert.NotNull(contextResolver);
    }

    // Test helper classes
    private class TestProvider : IAIProvider
    {
        public TestProvider(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public AICapabilities Capabilities { get; } = new() { MaxTokens = 1000 };

        public Task<AIResponse> GenerateAsync(AIRequest request, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new AIResponse
            {
                Success = true,
                                 Content = $"Response from {Name}",
                 Usage = new UsageMetrics(),
                 EstimatedCost = 0
            });
        }

        public Task<AIResponse> GenerateWithContextAsync(AIRequest request, IConversationContext context, CancellationToken cancellationToken = default)
        {
            return GenerateAsync(request, cancellationToken);
        }

        public Task<decimal> EstimateCostAsync(AIRequest request) => Task.FromResult(0m);
        public Task<bool> HealthCheckAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private class TestContextResolver : IContextResolver
    {
        public string ResolveContextId(IncomingMessage message) => "test-context";
        public TimeSpan GetDefaultExpiration(string channelType) => TimeSpan.FromHours(1);
        public int GetMaxMessageHistory(string channelType) => 10;
        public bool ShouldCreateNewContext(IncomingMessage message) => false;
    }

    private interface ITestService { }
    private class TestService : ITestService { }
} 