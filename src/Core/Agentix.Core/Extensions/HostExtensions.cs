using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Extensions;

/// <summary>
/// Simplified host extensions for Agentix
/// </summary>
public static class HostExtensions
{
    /// <summary>
    /// Builds the host, starts Agentix, and runs the application.
    /// This provides a fluent API: builder.BuildAndRunAgentixAsync()
    /// </summary>
    /// <param name="builder">The host builder</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the running application</returns>
    public static async Task BuildAndRunAgentixAsync(this IHostBuilder builder, CancellationToken cancellationToken = default)
    {
        using var host = builder.Build();
        await host.RunAgentixAsync(cancellationToken);
    }
    
    /// <summary>
    /// Starts Agentix channels and runs the application.
    /// This is the main entry point for Agentix applications.
    /// </summary>
    /// <param name="host">The configured host</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that represents the running application</returns>
    public static async Task RunAgentixAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        var logger = host.Services.GetService<ILogger<IHost>>();
        
        try
        {
            logger?.LogInformation("🚀 Starting Agentix...");
            
            // Start all registered channels
            await StartAgentixChannelsAsync(host, cancellationToken);
            
            logger?.LogInformation("✅ Agentix started successfully");
            
            // Run the host application
            await host.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Failed to start Agentix");
            throw;
        }
    }
    
    /// <summary>
    /// Starts Agentix channels only, without running the host.
    /// Useful when you want to start Agentix but handle the host lifecycle yourself.
    /// </summary>
    /// <param name="host">The configured host</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when Agentix channels are started</returns>
    public static async Task StartAgentixAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        var logger = host.Services.GetService<ILogger<IHost>>();
        
        try
        {
            logger?.LogInformation("🚀 Starting Agentix channels...");
            
            // Start all registered channels
            await StartAgentixChannelsAsync(host, cancellationToken);
            
            logger?.LogInformation("✅ Agentix channels started successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Failed to start Agentix channels");
            throw;
        }
    }

    /// <summary>
    /// Gets basic information about the Agentix system.
    /// </summary>
    /// <param name="host">The configured host</param>
    /// <returns>Basic system information</returns>
    public static AgentixSystemInfo GetAgentixInfo(this IHost host)
    {
        var providers = host.Services.GetServices<IAIProvider>();
        var channels = host.Services.GetServices<IChannelAdapter>();

        return new AgentixSystemInfo
        {
            AvailableProviders = providers.Select(p => p.Name).ToList(),
            AvailableChannels = channels.Select(c => c.Name).ToList(),
            RunningChannels = channels.Where(c => c.IsRunning).Select(c => c.Name).ToList()
        };
    }

    private static async Task StartAgentixChannelsAsync(IHost host, CancellationToken cancellationToken)
    {
        var logger = host.Services.GetService<ILogger<IHost>>();
        var channels = host.Services.GetServices<IChannelAdapter>();

        var startTasks = channels.Select(async channel =>
        {
            try
            {
                await channel.StartAsync(cancellationToken);
                logger?.LogInformation("Started channel: {ChannelName}", channel.Name);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to start channel: {ChannelName}", channel.Name);
            }
        });

        await Task.WhenAll(startTasks);
    }
}

/// <summary>
/// Basic system information for Agentix
/// </summary>
public class AgentixSystemInfo
{
    /// <summary>
    /// Gets or sets the list of available AI provider names in the system.
    /// </summary>
    /// <value>A list of provider names (e.g., "claude", "openai") that are registered and available for use.</value>
    public List<string> AvailableProviders { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of available channel adapter names in the system.
    /// </summary>
    /// <value>A list of channel names (e.g., "console", "slack", "teams") that are registered and available for use.</value>
    public List<string> AvailableChannels { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the list of channel adapter names that are currently running.
    /// </summary>
    /// <value>A list of channel names that are actively running and processing messages.</value>
    public List<string> RunningChannels { get; set; } = new();
} 