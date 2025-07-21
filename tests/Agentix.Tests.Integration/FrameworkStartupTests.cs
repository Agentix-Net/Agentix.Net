using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Context.InMemory.Extensions;
using Agentix.Tests.Integration.TestDoubles;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Agentix.Tests.Integration;

/// <summary>
/// Tests for framework startup and lifecycle management
/// </summary>
public class FrameworkStartupTests
{
    [Fact]
    public async Task StartAgentixAsync_WithMockChannel_StartsChannelSuccessfully()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();
        var channel = host.Services.GetRequiredService<IChannelAdapter>() as MockChannelAdapter;

        // Act
        await host.StartAgentixAsync();

        // Assert
        Assert.NotNull(channel);
        Assert.True(channel.IsRunning);
        Assert.Equal(1, channel.StartCallCount);
    }

    [Fact]
    public async Task StartAgentixAsync_WithFailingChannel_HandlesGracefully()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();
        var channel = host.Services.GetRequiredService<IChannelAdapter>() as MockChannelAdapter;
        channel!.ShouldFailStart = true;

        // Act & Assert - Should not throw, handles gracefully
        await host.StartAgentixAsync();
        
        // Channel should have been attempted to start
        Assert.Equal(1, channel.StartCallCount);
        Assert.False(channel.IsRunning); // Should remain stopped due to failure
    }

    [Fact]
    public void GetAgentixInfo_WithRegisteredComponents_ReturnsCorrectInformation()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();

        // Act
        var info = host.GetAgentixInfo();

        // Assert
        Assert.Contains("mock-ai", info.AvailableProviders);
        Assert.Contains("mock-channel", info.AvailableChannels);
        Assert.Empty(info.RunningChannels); // Not started yet
    }

    [Fact]
    public async Task GetAgentixInfo_AfterStartup_ShowsRunningChannels()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();

        // Act
        await host.StartAgentixAsync();
        var info = host.GetAgentixInfo();

        // Assert
        Assert.Contains("mock-ai", info.AvailableProviders);
        Assert.Contains("mock-channel", info.AvailableChannels);
        Assert.Contains("mock-channel", info.RunningChannels);
    }

    [Fact]
    public async Task StartAgentixAsync_WithMultipleChannels_StartsAllChannels()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        var channel1 = new MockChannelAdapter();
        var channel2 = new TestChannelAdapter("test-channel-2");
        
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel(channel1)
                .AddChannel(channel2);
        });

        using var host = builder.Build();

        // Act
        await host.StartAgentixAsync();

        // Assert
        Assert.True(channel1.IsRunning);
        Assert.True(channel2.IsRunning);
        Assert.Equal(1, channel1.StartCallCount);
        Assert.Equal(1, channel2.StartCallCount);
    }

    [Fact]
    public async Task FrameworkStartup_WithTimeoutCancellation_CompletesWithinTimeout()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        // Act & Assert - Should complete within timeout
        await host.StartAgentixAsync(cts.Token);
        
        var info = host.GetAgentixInfo();
        Assert.NotEmpty(info.RunningChannels);
    }

    [Fact]
    public void ServiceRegistration_StatelessMode_ResolvesCoreDependencies()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();

        // Act & Assert - Core services should resolve without throwing
        var orchestrator = host.Services.GetRequiredService<IAgentixOrchestrator>();
        var provider = host.Services.GetRequiredService<IAIProvider>();
        var channel = host.Services.GetRequiredService<IChannelAdapter>();

        Assert.NotNull(orchestrator);
        Assert.NotNull(provider);
        Assert.NotNull(channel);
        
        // Context services should NOT be available in stateless mode
        Assert.Null(host.Services.GetService<IContextResolver>());
        Assert.Null(host.Services.GetService<IContextStore>());
    }

    [Fact]
    public void ServiceRegistration_StatefulMode_ResolvesAllDependencies()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddInMemoryContext()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();

        // Act & Assert - All services should resolve without throwing
        var orchestrator = host.Services.GetRequiredService<IAgentixOrchestrator>();
        var provider = host.Services.GetRequiredService<IAIProvider>();
        var channel = host.Services.GetRequiredService<IChannelAdapter>();
        var contextResolver = host.Services.GetRequiredService<IContextResolver>();
        var contextStore = host.Services.GetRequiredService<IContextStore>();

        Assert.NotNull(orchestrator);
        Assert.NotNull(provider);
        Assert.NotNull(channel);
        Assert.NotNull(contextResolver);
        Assert.NotNull(contextStore);
    }

    [Fact]
    public async Task StartAgentixAsync_StatelessMode_WorksCorrectly()
    {
        // Arrange
        var builder = Host.CreateDefaultBuilder();
        builder.ConfigureServices(services =>
        {
            services.AddAgentix()
                .AddProvider<MockAIProvider>()
                .AddChannel<MockChannelAdapter>();
        });

        using var host = builder.Build();

        // Act & Assert - Should start successfully without context services
        await host.StartAgentixAsync();
        
        var info = host.GetAgentixInfo();
        Assert.Contains("mock-ai", info.AvailableProviders);
        Assert.Contains("mock-channel", info.AvailableChannels);
        Assert.Contains("mock-channel", info.RunningChannels);
    }

    // Test helper class
    private class TestChannelAdapter : IChannelAdapter
    {
        public TestChannelAdapter(string name)
        {
            Name = name;
        }

        public string Name { get; }
        public string ChannelType => Name;
        public bool IsRunning { get; private set; }
        public bool SupportsRichContent => false;
        public bool SupportsFileUploads => false;
        public bool SupportsInteractiveElements => false;

        public int StartCallCount { get; private set; }
        public int StopCallCount { get; private set; }

        public Task StartAsync(CancellationToken cancellationToken = default)
        {
            StartCallCount++;
            IsRunning = true;
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            StopCallCount++;
            IsRunning = false;
            return Task.CompletedTask;
        }

        public Task<bool> CanHandleAsync(IncomingMessage message) => Task.FromResult(true);
        
        public Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new ChannelResponse { Success = true });
        }

        public Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
} 