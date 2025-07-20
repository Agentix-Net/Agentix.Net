using Agentix.Core.Extensions;
using Agentix.Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Agentix.Channels.Slack.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Slack channel adapter to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configure">Optional configuration action for SlackChannelOptions</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddSlackChannel(this IServiceCollection services, Action<SlackChannelOptions>? configure = null)
    {
        var options = new SlackChannelOptions();
        configure?.Invoke(options);
        
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
        
        services.AddSingleton(options);
        services.AddSingleton<IChannelAdapter, SlackChannelAdapter>();
        
        return services;
    }
} 