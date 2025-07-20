using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Agentix.Core.Extensions;

public static class HostExtensions
{
    /// <summary>
    /// Builds the host and starts the Agentix orchestrator, then runs the application.
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
    /// Starts the Agentix orchestrator and runs the application.
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
            
            // Get and start the orchestrator
            var orchestrator = host.Services.GetRequiredService<IAgentixOrchestrator>();
            await orchestrator.StartAsync(cancellationToken);
            
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
    /// Starts the Agentix orchestrator only, without running the host.
    /// Useful when you want to start Agentix but handle the host lifecycle yourself.
    /// </summary>
    /// <param name="host">The configured host</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A task that completes when Agentix is started</returns>
    public static async Task StartAgentixAsync(this IHost host, CancellationToken cancellationToken = default)
    {
        var logger = host.Services.GetService<ILogger<IHost>>();
        
        try
        {
            logger?.LogInformation("🚀 Starting Agentix orchestrator...");
            
            // Get and start the orchestrator
            var orchestrator = host.Services.GetRequiredService<IAgentixOrchestrator>();
            await orchestrator.StartAsync(cancellationToken);
            
            logger?.LogInformation("✅ Agentix orchestrator started successfully");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "❌ Failed to start Agentix orchestrator");
            throw;
        }
    }
    
    /// <summary>
    /// Gets the health status of the Agentix system.
    /// </summary>
    /// <param name="host">The configured host</param>
    /// <returns>The system status</returns>
    public static async Task<SystemStatus> GetAgentixStatusAsync(this IHost host)
    {
        var orchestrator = host.Services.GetRequiredService<IAgentixOrchestrator>();
        return await orchestrator.GetStatusAsync();
    }
} 