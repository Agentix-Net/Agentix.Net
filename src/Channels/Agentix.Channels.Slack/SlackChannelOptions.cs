namespace Agentix.Channels.Slack;

public enum SlackChannelMode
{
    /// <summary>
    /// Use HTTP webhooks (requires public HTTPS endpoint)
    /// </summary>
    Webhook,
    
    /// <summary>
    /// Use WebSocket connections (works behind firewalls, great for development)
    /// </summary>
    SocketMode
}

public class SlackChannelOptions
{
    /// <summary>
    /// The connection mode to use (Webhook or SocketMode)
    /// </summary>
    public SlackChannelMode Mode { get; set; } = SlackChannelMode.Webhook;

    /// <summary>
    /// The Slack bot token (xoxb-...) for authenticating with Slack API
    /// </summary>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// The signing secret for verifying Slack webhook requests (required for Webhook mode)
    /// </summary>
    public string SigningSecret { get; set; } = string.Empty;

    /// <summary>
    /// The app-level token (xapp-...) for Socket Mode authentication (required for SocketMode)
    /// </summary>
    public string AppToken { get; set; } = string.Empty;

    /// <summary>
    /// The port to listen on for incoming Slack webhooks (defaults to 3000)
    /// </summary>
    public int WebhookPort { get; set; } = 3000;

    /// <summary>
    /// The path for the Slack events webhook endpoint (defaults to /slack/events)
    /// </summary>
    public string WebhookPath { get; set; } = "/slack/events";

    /// <summary>
    /// Whether to respond to mentions only or all messages in channels the bot has access to
    /// </summary>
    public bool RespondToMentionsOnly { get; set; } = true;

    /// <summary>
    /// Whether to respond to direct messages
    /// </summary>
    public bool RespondToDirectMessages { get; set; } = true;

    /// <summary>
    /// Maximum length for responses (Slack has a 4000 character limit for messages)
    /// </summary>
    public int MaxResponseLength { get; set; } = 3900;

    /// <summary>
    /// Whether to use threading for responses in channels
    /// </summary>
    public bool UseThreading { get; set; } = true;

    /// <summary>
    /// Whether to include usage metadata in responses (for debugging/monitoring)
    /// </summary>
    public bool ShowMetadata { get; set; } = false;

    /// <summary>
    /// Custom bot name to display in Slack (if different from configured bot user)
    /// </summary>
    public string? BotName { get; set; }

    /// <summary>
    /// Custom emoji to use as bot icon
    /// </summary>
    public string? BotEmoji { get; set; }

    /// <summary>
    /// List of channel IDs where the bot should operate (empty means all accessible channels)
    /// </summary>
    public List<string> AllowedChannels { get; set; } = new();

    /// <summary>
    /// List of user IDs that are allowed to interact with the bot (empty means all users)
    /// </summary>
    public List<string> AllowedUsers { get; set; } = new();

    /// <summary>
    /// Whether to log incoming messages for debugging
    /// </summary>
    public bool LogMessages { get; set; } = false;

    /// <summary>
    /// Timeout for Slack API calls in seconds
    /// </summary>
    public int ApiTimeoutSeconds { get; set; } = 30;

    // Socket Mode specific options

    /// <summary>
    /// Interval for sending ping messages to keep the WebSocket connection alive (Socket Mode only)
    /// </summary>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum time to wait for reconnection attempts (Socket Mode only)
    /// </summary>
    public TimeSpan ReconnectTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Maximum number of reconnection attempts before giving up (Socket Mode only)
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 10;

    /// <summary>
    /// Whether to automatically reconnect when the WebSocket connection is lost (Socket Mode only)
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Whether to log WebSocket connection events for debugging (Socket Mode only)
    /// </summary>
    public bool LogConnectionEvents { get; set; } = false;
} 