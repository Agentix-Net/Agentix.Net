using System.Collections.Concurrent;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

public class ChannelRegistry : IChannelRegistry
{
    private readonly ConcurrentDictionary<string, IChannelAdapter> _channels = new();
    private readonly ILogger<ChannelRegistry> _logger;

    public ChannelRegistry(ILogger<ChannelRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterChannel(IChannelAdapter channel)
    {
        if (string.IsNullOrEmpty(channel.Name))
        {
            throw new ArgumentException("Channel name cannot be null or empty", nameof(channel));
        }

        _channels.AddOrUpdate(channel.Name, channel, (key, oldValue) => channel);
        _logger.LogInformation("Registered channel adapter: {ChannelName} (Type: {ChannelType})", 
                             channel.Name, channel.ChannelType);
    }

    public IChannelAdapter? GetChannel(string name)
    {
        _channels.TryGetValue(name, out var channel);
        return channel;
    }

    public IEnumerable<IChannelAdapter> GetAllChannels()
    {
        return _channels.Values;
    }

    public IEnumerable<IChannelAdapter> GetRunningChannels()
    {
        return _channels.Values.Where(c => c.IsRunning);
    }

    public bool IsChannelRunning(string name)
    {
        var channel = GetChannel(name);
        return channel?.IsRunning ?? false;
    }

    public async Task StartAllChannelsAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _channels.Values.Select(channel => StartChannelSafeAsync(channel, cancellationToken));
        await Task.WhenAll(tasks);
    }

    public async Task StopAllChannelsAsync(CancellationToken cancellationToken = default)
    {
        var tasks = _channels.Values.Where(c => c.IsRunning)
                                   .Select(channel => StopChannelSafeAsync(channel, cancellationToken));
        await Task.WhenAll(tasks);
    }

    private async Task StartChannelSafeAsync(IChannelAdapter channel, CancellationToken cancellationToken)
    {
        try
        {
            await channel.StartAsync(cancellationToken);
            _logger.LogInformation("Started channel: {ChannelName}", channel.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start channel: {ChannelName}", channel.Name);
        }
    }

    private async Task StopChannelSafeAsync(IChannelAdapter channel, CancellationToken cancellationToken)
    {
        try
        {
            await channel.StopAsync(cancellationToken);
            _logger.LogInformation("Stopped channel: {ChannelName}", channel.Name);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop channel: {ChannelName}", channel.Name);
        }
    }
} 