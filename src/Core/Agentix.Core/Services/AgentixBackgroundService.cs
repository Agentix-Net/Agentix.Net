using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

public class AgentixBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<AgentixBackgroundService> _logger;
    private IProviderRegistry? _providerRegistry;
    private IChannelRegistry? _channelRegistry;

    public AgentixBackgroundService(IServiceProvider serviceProvider, ILogger<AgentixBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting Agentix background service...");

            // Get registries
            _providerRegistry = _serviceProvider.GetRequiredService<IProviderRegistry>();
            _channelRegistry = _serviceProvider.GetRequiredService<IChannelRegistry>();

            // Register all AI providers from DI container
            var providers = _serviceProvider.GetServices<IAIProvider>();
            foreach (var provider in providers)
            {
                _providerRegistry.RegisterProvider(provider);
            }

            // Register all channel adapters from DI container
            var channels = _serviceProvider.GetServices<IChannelAdapter>();
            foreach (var channel in channels)
            {
                _channelRegistry.RegisterChannel(channel);
            }

            _logger.LogInformation("Agentix background service started. Providers: {ProviderCount}, Channels: {ChannelCount}",
                                 providers.Count(), channels.Count());

            // Keep the service running
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Agentix background service is stopping...");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in Agentix background service");
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping Agentix background service...");

        // Stop all channels gracefully
        if (_channelRegistry != null)
        {
            var channelRegistry = _channelRegistry as ChannelRegistry;
            if (channelRegistry != null)
            {
                await channelRegistry.StopAllChannelsAsync(cancellationToken);
            }
        }

        await base.StopAsync(cancellationToken);
        _logger.LogInformation("Agentix background service stopped");
    }
} 