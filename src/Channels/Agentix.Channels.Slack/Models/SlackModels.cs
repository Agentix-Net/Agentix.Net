using System.Text.Json;
using System.Text.Json.Serialization;

namespace Agentix.Channels.Slack.Models;

/// <summary>
/// Internal model representing a Slack event callback structure.
/// </summary>
internal class SlackEventCallback
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
/// Internal model representing Slack event data.
/// </summary>
internal class SlackEvent
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
/// Internal model representing a Slack message post request.
/// </summary>
internal class SlackMessageRequest
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
/// Internal model representing a Slack API response.
/// </summary>
internal class SlackApiResponse
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
/// Internal model representing a Slack user info response.
/// </summary>
internal class SlackUserInfoResponse : SlackApiResponse
{
    [JsonPropertyName("user")]
    public SlackUser? User { get; set; }
}

/// <summary>
/// Internal model representing Slack user data.
/// </summary>
internal class SlackUser
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
/// Internal model representing a Slack channel info response.
/// </summary>
internal class SlackChannelInfoResponse : SlackApiResponse
{
    [JsonPropertyName("channel")]
    public new SlackChannel? Channel { get; set; }
}

/// <summary>
/// Internal model representing Slack channel data.
/// </summary>
internal class SlackChannel
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

/// <summary>
/// Internal model representing Socket Mode connection response from apps.connections.open.
/// </summary>
internal class SocketModeConnectionResponse : SlackApiResponse
{
    [JsonPropertyName("url")]
    public string? Url { get; set; }
}

/// <summary>
/// Internal model representing Socket Mode envelope for all WebSocket messages.
/// </summary>
internal class SocketModeEnvelope
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
/// Internal model representing Socket Mode acknowledgment message.
/// </summary>
internal class SocketModeAck
{
    [JsonPropertyName("envelope_id")]
    public string EnvelopeId { get; set; } = string.Empty;

    [JsonPropertyName("payload")]
    public object? Payload { get; set; }
}

/// <summary>
/// Internal model representing Socket Mode ping message.
/// </summary>
internal class SocketModePing
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "ping";

    [JsonPropertyName("id")]
    public int Id { get; set; }
}

/// <summary>
/// Internal model representing Socket Mode pong response.
/// </summary>
internal class SocketModePong
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = "pong";

    [JsonPropertyName("reply_to")]
    public int ReplyTo { get; set; }
} 