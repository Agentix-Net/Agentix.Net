using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agentix.Channels.Slack.Models;

/// <summary>
/// Slack event callback structure
/// </summary>
public class SlackEventCallback
{
    [JsonPropertyName("token")]
    public string Token { get; set; } = string.Empty;

    [JsonPropertyName("team_id")]
    public string TeamId { get; set; } = string.Empty;

    [JsonPropertyName("api_app_id")]
    public string ApiAppId { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public SlackEvent Event { get; set; } = new();

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("event_id")]
    public string EventId { get; set; } = string.Empty;

    [JsonPropertyName("event_time")]
    public long EventTime { get; set; }

    [JsonPropertyName("challenge")]
    public string? Challenge { get; set; }
}

/// <summary>
/// Slack event data
/// </summary>
public class SlackEvent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("user")]
    public string User { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("ts")]
    public string Timestamp { get; set; } = string.Empty;

    [JsonPropertyName("event_ts")]
    public string EventTimestamp { get; set; } = string.Empty;

    [JsonPropertyName("channel_type")]
    public string ChannelType { get; set; } = string.Empty;

    [JsonPropertyName("bot_id")]
    public string? BotId { get; set; }

    [JsonPropertyName("thread_ts")]
    public string? ThreadTimestamp { get; set; }
}

/// <summary>
/// Slack message post request
/// </summary>
public class SlackMessageRequest
{
    [JsonPropertyName("channel")]
    public string Channel { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("thread_ts")]
    public string? ThreadTimestamp { get; set; }

    [JsonPropertyName("username")]
    public string? Username { get; set; }

    [JsonPropertyName("icon_emoji")]
    public string? IconEmoji { get; set; }

    [JsonPropertyName("as_user")]
    public bool AsUser { get; set; } = true;
}

/// <summary>
/// Slack API response
/// </summary>
public class SlackApiResponse
{
    [JsonPropertyName("ok")]
    public bool Ok { get; set; }

    [JsonPropertyName("error")]
    public string? Error { get; set; }

    [JsonPropertyName("ts")]
    public string? Timestamp { get; set; }

    [JsonPropertyName("channel")]
    public string? Channel { get; set; }
}

/// <summary>
/// Slack user info response
/// </summary>
public class SlackUserInfoResponse : SlackApiResponse
{
    [JsonPropertyName("user")]
    public SlackUser? User { get; set; }
}

/// <summary>
/// Slack user data
/// </summary>
public class SlackUser
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("real_name")]
    public string RealName { get; set; } = string.Empty;

    [JsonPropertyName("display_name")]
    public string DisplayName { get; set; } = string.Empty;

    [JsonPropertyName("is_bot")]
    public bool IsBot { get; set; }
}

/// <summary>
/// Slack channel info response
/// </summary>
public class SlackChannelInfoResponse : SlackApiResponse
{
    [JsonPropertyName("channel")]
    public new SlackChannel? Channel { get; set; }
}

/// <summary>
/// Slack channel data
/// </summary>
public class SlackChannel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("is_channel")]
    public bool IsChannel { get; set; }

    [JsonPropertyName("is_group")]
    public bool IsGroup { get; set; }

    [JsonPropertyName("is_im")]
    public bool IsIm { get; set; }
}

// Socket Mode specific models

/// <summary>
/// Socket Mode connection response from apps.connections.open
/// </summary>
public class SocketModeConnectionResponse : SlackApiResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Socket Mode envelope for all WebSocket messages
/// </summary>
public class SocketModeEnvelope
{
    [JsonPropertyName("envelope_id")]
    public string EnvelopeId { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("accepts_response_payload")]
    public bool AcceptsResponsePayload { get; set; }

    [JsonPropertyName("payload")]
    public JsonElement? Payload { get; set; }

    [JsonPropertyName("retry_attempt")]
    public int? RetryAttempt { get; set; }

    [JsonPropertyName("retry_reason")]
    public string? RetryReason { get; set; }
}

/// <summary>
/// Socket Mode acknowledgment message
/// </summary>
public class SocketModeAck
{
    [JsonPropertyName("envelope_id")]
    public string EnvelopeId { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }
}

/// <summary>
/// Socket Mode ping message
/// </summary>
public class SocketModePing
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "ping";

    [JsonPropertyName("id")]
    public int Id { get; set; }
}

/// <summary>
/// Socket Mode pong response
/// </summary>
public class SocketModePong
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "pong";

    [JsonPropertyName("reply_to")]
    public int ReplyTo { get; set; }
} 