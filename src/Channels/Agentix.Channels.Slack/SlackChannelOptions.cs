namespace Agentix.Channels.Slack;

/// <summary>
/// Specifies the connection method for communicating with Slack.
/// </summary>
public enum SlackChannelMode
{
    /// <summary>
    /// Use HTTP webhooks for receiving events from Slack.
    /// Requires a publicly accessible HTTPS endpoint but offers better scalability.
    /// </summary>
    /// <remarks>
    /// Webhook mode is ideal for production environments with proper infrastructure.
    /// Slack sends HTTP POST requests to your configured endpoint when events occur.
    /// </remarks>
    Webhook,
    
    /// <summary>
    /// Use WebSocket connections via Slack's Socket Mode for real-time communication.
    /// Works behind firewalls and NAT, making it perfect for development environments.
    /// </summary>
    /// <remarks>
    /// Socket Mode maintains a persistent WebSocket connection to Slack's servers.
    /// This mode is excellent for development and environments without public endpoints.
    /// </remarks>
    SocketMode
}

/// <summary>
/// Configuration options for the Slack channel adapter.
/// Contains settings for connection methods, bot behavior, security, and user interaction preferences.
/// </summary>
public sealed class SlackChannelOptions
{
    /// <summary>
    /// Gets or sets the connection mode to use for communicating with Slack.
    /// </summary>
    /// <value>The connection mode. Defaults to <see cref="SlackChannelMode.Webhook"/>.</value>
    /// <remarks>
    /// Choose <see cref="SlackChannelMode.Webhook"/> for production environments with public endpoints,
    /// or <see cref="SlackChannelMode.SocketMode"/> for development or environments behind firewalls.
    /// </remarks>
    public SlackChannelMode Mode { get; set; } = SlackChannelMode.Webhook;

    /// <summary>
    /// Gets or sets the Slack bot token for authenticating with the Slack API.
    /// </summary>
    /// <value>The bot token starting with "xoxb-". This field is required.</value>
    /// <remarks>
    /// Obtain this token from your Slack app's "OAuth and Permissions" page after installing
    /// the app to your workspace. The token should have appropriate bot token scopes
    /// such as chat:write, channels:history, and users:read.
    /// </remarks>
    public string BotToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the signing secret for verifying Slack webhook requests.
    /// </summary>
    /// <value>The signing secret string. Required for webhook mode.</value>
    /// <remarks>
    /// This secret is used to verify that webhook requests actually come from Slack
    /// and haven't been tampered with. Find this in your Slack app's "Basic Information" page.
    /// Required when <see cref="Mode"/> is <see cref="SlackChannelMode.Webhook"/>.
    /// </remarks>
    public string SigningSecret { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the app-level token for Socket Mode authentication.
    /// </summary>
    /// <value>The app-level token starting with "xapp-". Required for Socket Mode.</value>
    /// <remarks>
    /// Create this token in your Slack app's "Socket Mode" page with the connections:write scope.
    /// This token allows your application to establish a WebSocket connection to Slack.
    /// Required when <see cref="Mode"/> is <see cref="SlackChannelMode.SocketMode"/>.
    /// </remarks>
    public string AppToken { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the port to listen on for incoming Slack webhooks.
    /// </summary>
    /// <value>The port number. Defaults to 3000.</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.Webhook"/>.
    /// Ensure this port is accessible from the internet and configure your Slack app's
    /// Event Subscriptions URL accordingly.
    /// </remarks>
    public int WebhookPort { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the URL path for the Slack events webhook endpoint.
    /// </summary>
    /// <value>The webhook path. Defaults to "/slack/events".</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.Webhook"/>.
    /// This should match the path configured in your Slack app's Event Subscriptions URL.
    /// For example: https://yourdomain.com:3000/slack/events
    /// </remarks>
    public string WebhookPath { get; set; } = "/slack/events";

    /// <summary>
    /// Gets or sets a value indicating whether the bot should only respond when mentioned in channels.
    /// </summary>
    /// <value>True to respond only to mentions; false to respond to all messages. Defaults to true.</value>
    /// <remarks>
    /// When true, the bot will only respond to messages that explicitly mention it (e.g., @botname).
    /// When false, the bot will respond to all messages in channels it has access to.
    /// Direct messages are controlled separately by <see cref="RespondToDirectMessages"/>.
    /// </remarks>
    public bool RespondToMentionsOnly { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the bot should respond to direct messages.
    /// </summary>
    /// <value>True to respond to direct messages; otherwise, false. Defaults to true.</value>
    /// <remarks>
    /// Direct messages are private conversations between a user and the bot.
    /// This setting is independent of <see cref="RespondToMentionsOnly"/>.
    /// </remarks>
    public bool RespondToDirectMessages { get; set; } = true;

    /// <summary>
    /// Gets or sets the maximum length for bot responses.
    /// </summary>
    /// <value>The maximum response length in characters. Defaults to 3900.</value>
    /// <remarks>
    /// Slack has a 4000 character limit for messages. Setting this slightly lower (3900)
    /// provides buffer space for metadata and formatting. Longer responses will be truncated
    /// with a note indicating the truncation.
    /// </remarks>
    public int MaxResponseLength { get; set; } = 3900;

    /// <summary>
    /// Gets or sets a value indicating whether to use threading for bot responses in channels.
    /// </summary>
    /// <value>True to use threads; otherwise, false. Defaults to true.</value>
    /// <remarks>
    /// When enabled, bot responses will be posted as replies in threads, keeping
    /// conversations organized. Disable this if you prefer responses to appear
    /// as new messages in the channel.
    /// </remarks>
    public bool UseThreading { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to include usage metadata in bot responses.
    /// </summary>
    /// <value>True to show metadata such as token usage and costs; otherwise, false. Defaults to false.</value>
    /// <remarks>
    /// When enabled, responses will include information like token counts and estimated costs.
    /// This is useful for debugging and monitoring but may not be desired in production.
    /// Example: "📊 Tokens: 45 in, 128 out ($0.0023)"
    /// </remarks>
    public bool ShowMetadata { get; set; } = false;

    /// <summary>
    /// Gets or sets a custom bot name to display in Slack.
    /// </summary>
    /// <value>The custom bot name, or null to use the configured bot user name.</value>
    /// <remarks>
    /// This overrides the bot's configured username for display purposes.
    /// Useful for providing context-specific names or branding.
    /// </remarks>
    public string? BotName { get; set; }

    /// <summary>
    /// Gets or sets a custom emoji to use as the bot's icon.
    /// </summary>
    /// <value>The emoji name (e.g., ":robot_face:"), or null to use the bot's configured icon.</value>
    /// <remarks>
    /// Use standard Slack emoji names including the colons.
    /// Examples: ":robot_face:", ":speech_balloon:", ":artificial_intelligence:"
    /// </remarks>
    public string? BotEmoji { get; set; }

    /// <summary>
    /// Gets or sets the list of channel IDs where the bot should operate.
    /// </summary>
    /// <value>A list of Slack channel IDs. Empty list means the bot operates in all accessible channels.</value>
    /// <remarks>
    /// Use this to restrict the bot to specific channels. Channel IDs typically start with "C"
    /// for public channels or "G" for private channels. You can find channel IDs in the Slack URL
    /// or through the Slack API.
    /// </remarks>
    public List<string> AllowedChannels { get; set; } = new();

    /// <summary>
    /// Gets or sets the list of user IDs that are allowed to interact with the bot.
    /// </summary>
    /// <value>A list of Slack user IDs. Empty list means all users can interact with the bot.</value>
    /// <remarks>
    /// Use this to restrict bot interactions to specific users. User IDs typically start with "U".
    /// You can find user IDs through the Slack API or various Slack tools and apps.
    /// </remarks>
    public List<string> AllowedUsers { get; set; } = new();

    /// <summary>
    /// Gets or sets a value indicating whether to log incoming messages for debugging purposes.
    /// </summary>
    /// <value>True to log messages; otherwise, false. Defaults to false.</value>
    /// <remarks>
    /// When enabled, incoming messages will be logged with user and channel information.
    /// Be cautious with this setting in production to avoid logging sensitive information.
    /// </remarks>
    public bool LogMessages { get; set; } = false;

    /// <summary>
    /// Gets or sets the timeout for Slack API calls.
    /// </summary>
    /// <value>The timeout duration in seconds. Defaults to 30 seconds.</value>
    /// <remarks>
    /// This timeout applies to all HTTP requests made to the Slack API, including
    /// sending messages and retrieving user/channel information.
    /// </remarks>
    public int ApiTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Gets or sets the interval for sending ping messages to keep the WebSocket connection alive.
    /// </summary>
    /// <value>The heartbeat interval. Defaults to 30 seconds.</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.SocketMode"/>.
    /// Ping messages help detect connection issues and prevent timeouts.
    /// </remarks>
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum time to wait between reconnection attempts.
    /// </summary>
    /// <value>The reconnection timeout. Defaults to 30 seconds.</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.SocketMode"/>.
    /// The actual delay uses exponential backoff, capped at this maximum value.
    /// </remarks>
    public TimeSpan ReconnectTimeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Gets or sets the maximum number of reconnection attempts before giving up.
    /// </summary>
    /// <value>The maximum reconnection attempts. Defaults to 10.</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.SocketMode"/>.
    /// After reaching this limit, the connection will be considered permanently failed.
    /// Set to -1 for unlimited attempts.
    /// </remarks>
    public int MaxReconnectAttempts { get; set; } = 10;

    /// <summary>
    /// Gets or sets a value indicating whether to automatically reconnect when the WebSocket connection is lost.
    /// </summary>
    /// <value>True to enable automatic reconnection; otherwise, false. Defaults to true.</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.SocketMode"/>.
    /// When enabled, the client will automatically attempt to reconnect after connection failures.
    /// </remarks>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to log WebSocket connection events for debugging.
    /// </summary>
    /// <value>True to log connection events; otherwise, false. Defaults to false.</value>
    /// <remarks>
    /// Only used when <see cref="Mode"/> is <see cref="SlackChannelMode.SocketMode"/>.
    /// When enabled, connection events like connect, disconnect, and reconnection attempts
    /// will be logged for troubleshooting purposes.
    /// </remarks>
    public bool LogConnectionEvents { get; set; } = false;
} 