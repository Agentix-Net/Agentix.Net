using Agentix.Core.Models;

namespace Agentix.Core.Interfaces;

/// <summary>
/// Resolves context boundaries and lifecycle for different channels
/// </summary>
public interface IContextResolver
{
    string ResolveContextId(IncomingMessage message);
    TimeSpan GetDefaultExpiration(string channelType);
    int GetMaxMessageHistory(string channelType);
    bool ShouldCreateNewContext(IncomingMessage message);
} 