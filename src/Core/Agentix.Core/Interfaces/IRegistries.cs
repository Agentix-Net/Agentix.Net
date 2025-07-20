using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

public interface IProviderRegistry
{
    void RegisterProvider(IAIProvider provider);
    IAIProvider? GetProvider(string name);
    IEnumerable<IAIProvider> GetAllProviders();
    IAIProvider? GetDefaultProvider();
    bool IsProviderAvailable(string name);
}

public interface IChannelRegistry
{
    void RegisterChannel(IChannelAdapter channel);
    IChannelAdapter? GetChannel(string name);
    IEnumerable<IChannelAdapter> GetAllChannels();
    IEnumerable<IChannelAdapter> GetRunningChannels();
    bool IsChannelRunning(string name);
} 