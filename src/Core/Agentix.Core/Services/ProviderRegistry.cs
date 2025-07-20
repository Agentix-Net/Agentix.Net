using System.Collections.Concurrent;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Services;

public class ProviderRegistry : IProviderRegistry
{
    private readonly ConcurrentDictionary<string, IAIProvider> _providers = new();
    private readonly ILogger<ProviderRegistry> _logger;
    private string? _defaultProviderName;

    public ProviderRegistry(ILogger<ProviderRegistry> logger)
    {
        _logger = logger;
    }

    public void RegisterProvider(IAIProvider provider)
    {
        if (string.IsNullOrEmpty(provider.Name))
        {
            throw new ArgumentException("Provider name cannot be null or empty", nameof(provider));
        }

        _providers.AddOrUpdate(provider.Name, provider, (key, oldValue) => provider);
        
        // Set first provider as default if no default is set
        if (_defaultProviderName == null)
        {
            _defaultProviderName = provider.Name;
        }

        _logger.LogInformation("Registered AI provider: {ProviderName}", provider.Name);
    }

    public IAIProvider? GetProvider(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return GetDefaultProvider();
        }

        _providers.TryGetValue(name, out var provider);
        return provider;
    }

    public IEnumerable<IAIProvider> GetAllProviders()
    {
        return _providers.Values;
    }

    public IAIProvider? GetDefaultProvider()
    {
        if (_defaultProviderName == null)
        {
            return _providers.Values.FirstOrDefault();
        }

        _providers.TryGetValue(_defaultProviderName, out var provider);
        return provider ?? _providers.Values.FirstOrDefault();
    }

    public bool IsProviderAvailable(string name)
    {
        return _providers.ContainsKey(name);
    }

    public void SetDefaultProvider(string name)
    {
        if (IsProviderAvailable(name))
        {
            _defaultProviderName = name;
            _logger.LogInformation("Set default AI provider to: {ProviderName}", name);
        }
        else
        {
            throw new ArgumentException($"Provider '{name}' is not registered", nameof(name));
        }
    }
} 