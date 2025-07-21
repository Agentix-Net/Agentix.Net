using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Channels.Slack.Extensions;

/// <summary>
/// Extension methods for adding Slack channel to Agentix
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Slack channel adapter to the Agentix configuration
    /// </summary>
    /// <param name="builder">The Agentix builder</param>
    /// <param name="configure">Optional configuration action for SlackChannelOptions</param>
    /// <returns>The Agentix builder for chaining</returns>
    public static AgentixBuilder AddSlackChannel(this AgentixBuilder builder, Action<SlackChannelOptions>? configure = null)
    {
        var options = new SlackChannelOptions();
        configure?.Invoke(options);
        
        ValidateSlackOptions(options);
        
        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<IChannelAdapter, SlackChannelAdapter>();
        
        return builder;
    }

    /// <summary>
    /// Adds Slack channel adapter to the service collection (for direct DI usage)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration action for SlackChannelOptions</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSlackChannel(this IServiceCollection services, Action<SlackChannelOptions>? configure = null)
    {
        var options = new SlackChannelOptions();
        configure?.Invoke(options);
        
        ValidateSlackOptions(options);
        
        services.AddSingleton(options);
        services.AddSingleton<IChannelAdapter, SlackChannelAdapter>();
        
        return services;
    }

    private static void ValidateSlackOptions(SlackChannelOptions options)
    {
        // Validate required options
        if (string.IsNullOrEmpty(options.BotToken))
        {
            throw new ArgumentException("Slack bot token is required. Set SlackChannelOptions.BotToken.");
        }
        
        // Validate mode-specific options
        if (options.Mode == SlackChannelMode.Webhook)
        {
            if (string.IsNullOrEmpty(options.SigningSecret))
            {
                throw new ArgumentException("Slack signing secret is required for Webhook mode. Set SlackChannelOptions.SigningSecret.");
            }
        }
        else if (options.Mode == SlackChannelMode.SocketMode)
        {
            if (string.IsNullOrEmpty(options.AppToken))
            {
                throw new ArgumentException("Slack app token is required for Socket Mode. Set SlackChannelOptions.AppToken.");
            }
        }
    }
} 