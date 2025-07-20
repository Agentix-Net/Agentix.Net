using Agentix.Core.Interfaces;
using Agentix.Core.Models;
using Agentix.Channels.Slack.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Hosting;
using System.Text.Json;
using System.Text;
using System.Security.Cryptography;

namespace Agentix.Channels.Slack;

public class SlackChannelAdapter : IChannelAdapter, IDisposable
{
    private readonly IAgentixOrchestrator _orchestrator;
    private readonly ILogger<SlackChannelAdapter> _logger;
    private readonly SlackChannelOptions _options;
    private readonly HttpClient _httpClient;
    private WebApplication? _webApp;
    private SocketModeClient? _socketModeClient;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _runningTask;
    private string? _botUserId;

    public string Name => "slack";
    public string ChannelType => "slack";
    public bool IsRunning { get; private set; }

    // Slack supports rich content and interactive elements
    public bool SupportsRichContent => true;
    public bool SupportsFileUploads => true;
    public bool SupportsInteractiveElements => true;

    public SlackChannelAdapter(
        IAgentixOrchestrator orchestrator,
        ILogger<SlackChannelAdapter> logger,
        SlackChannelOptions? options = null)
    {
        _orchestrator = orchestrator;
        _logger = logger;
        _options = options ?? new SlackChannelOptions();
        
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_options.ApiTimeoutSeconds)
        };
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.BotToken}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Agentix-Slack-Bot/0.0.1");

        // Create SocketModeClient if using Socket Mode
        if (_options.Mode == SlackChannelMode.SocketMode)
        {
            // Create a simple logger for SocketModeClient
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
            var socketLogger = loggerFactory.CreateLogger<SocketModeClient>();
            
            _socketModeClient = new SocketModeClient(_httpClient, socketLogger, _options);
            _socketModeClient.MessageReceived += OnSocketModeMessageReceived;
            _socketModeClient.Connected += OnSocketModeConnected;
            _socketModeClient.Disconnected += OnSocketModeDisconnected;
            _socketModeClient.ConnectionError += OnSocketModeConnectionError;
        }
    }

    public Task<bool> CanHandleAsync(IncomingMessage message)
    {
        // Slack channel can handle messages from Slack
        return Task.FromResult(message.Channel == ChannelType || message.Channel == "slack");
    }

    public async Task<ChannelResponse> ProcessAsync(IncomingMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Processing Slack message: {Content} from user {UserId} in channel {ChannelId}", 
                message.Content, message.UserId, message.ChannelId);

            // Process with AI orchestrator
            var aiResponse = await _orchestrator.ProcessMessageAsync(message, cancellationToken);

            return new ChannelResponse
            {
                Success = true,
                AIResponse = aiResponse
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Slack message");
            
            return new ChannelResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task SendResponseAsync(AIResponse response, MessageContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            if (!response.Success)
            {
                await SendSlackMessageAsync(context.ChannelId, 
                    $"❌ Error: {response.ErrorMessage}", 
                    GetThreadTimestamp(context),
                    cancellationToken);
                return;
            }

            var messageText = response.Content;
            
            // Truncate if too long
            if (messageText.Length > _options.MaxResponseLength)
            {
                messageText = messageText.Substring(0, _options.MaxResponseLength - 100) + 
                             "\n\n_[Message truncated due to length limit]_";
            }

            // Add metadata if configured
            if (_options.ShowMetadata && response.Usage.TotalTokens > 0)
            {
                messageText += $"\n\n_📊 Tokens: {response.Usage.InputTokens} in, {response.Usage.OutputTokens} out (${response.EstimatedCost:F4})_";
            }

            await SendSlackMessageAsync(context.ChannelId, messageText, GetThreadTimestamp(context), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack response");
        }
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (IsRunning)
        {
            _logger.LogWarning("Slack channel is already running");
            return;
        }

        if (string.IsNullOrEmpty(_options.BotToken))
        {
            throw new InvalidOperationException("Slack bot token is required");
        }

        // Validate mode-specific requirements
        if (_options.Mode == SlackChannelMode.Webhook && string.IsNullOrEmpty(_options.SigningSecret))
        {
            throw new InvalidOperationException("Slack signing secret is required for Webhook mode");
        }

        if (_options.Mode == SlackChannelMode.SocketMode && string.IsNullOrEmpty(_options.AppToken))
        {
            throw new InvalidOperationException("Slack app token is required for Socket Mode");
        }

        _logger.LogInformation("Starting Slack channel in {Mode} mode...", _options.Mode);

        try
        {
            // Get bot user ID
            _botUserId = await GetBotUserIdAsync(cancellationToken);
            _logger.LogInformation("Bot user ID: {BotUserId}", _botUserId);

            _cancellationTokenSource = new CancellationTokenSource();
            IsRunning = true;

            if (_options.Mode == SlackChannelMode.Webhook)
            {
                await StartWebhookModeAsync(cancellationToken);
            }
            else if (_options.Mode == SlackChannelMode.SocketMode)
            {
                await StartSocketModeAsync(cancellationToken);
            }

            _logger.LogInformation("Slack channel started successfully in {Mode} mode", _options.Mode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start Slack channel");
            IsRunning = false;
            throw;
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken = default)
    {
        if (!IsRunning)
        {
            _logger.LogWarning("Slack channel is not running");
            return;
        }

        _logger.LogInformation("Stopping Slack channel...");

        IsRunning = false;
        _cancellationTokenSource?.Cancel();

        if (_options.Mode == SlackChannelMode.Webhook && _webApp != null)
        {
            await _webApp.StopAsync(cancellationToken);
            await _webApp.DisposeAsync();
            _webApp = null;
        }

        if (_options.Mode == SlackChannelMode.SocketMode && _socketModeClient != null)
        {
            await _socketModeClient.DisconnectAsync(cancellationToken);
        }

        if (_runningTask != null)
        {
            try
            {
                await _runningTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when cancelling
            }
        }

        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;
        _runningTask = null;

        _logger.LogInformation("Slack channel stopped");
    }

    private async Task StartWebhookModeAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting webhook server on port {Port}...", _options.WebhookPort);

        // Setup webhook server
        var builder = WebApplication.CreateBuilder();
        
        _webApp = builder.Build();
        _webApp.Urls.Add($"http://0.0.0.0:{_options.WebhookPort}");

        // Configure webhook endpoint
        _webApp.MapPost(_options.WebhookPath, async (HttpContext context) =>
        {
            return await HandleSlackWebhookAsync(context);
        });

        // Health check endpoint
        _webApp.MapGet("/health", () => "OK");

        // Start web server
        _runningTask = Task.Run(async () =>
        {
            try
            {
                await _webApp.RunAsync(_cancellationTokenSource!.Token);
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
            }
        }, cancellationToken);

        _logger.LogInformation("Webhook server started on {WebhookPath}", _options.WebhookPath);
    }

    private async Task StartSocketModeAsync(CancellationToken cancellationToken)
    {
        if (_socketModeClient == null)
        {
            throw new InvalidOperationException("Socket Mode client not initialized");
        }

        _logger.LogInformation("Connecting to Slack Socket Mode...");
        await _socketModeClient.ConnectAsync(cancellationToken);
    }

    // Socket Mode event handlers

    private async Task OnSocketModeMessageReceived(SocketModeEnvelope envelope)
    {
        try
        {
            if (envelope.Type == "events_api" && envelope.Payload.HasValue)
            {
                var eventCallback = JsonSerializer.Deserialize<SlackEventCallback>(envelope.Payload.Value);
                if (eventCallback?.Event != null)
                {
                    await HandleMessageEventAsync(eventCallback.Event);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Socket Mode message");
        }
    }

    private Task OnSocketModeConnected()
    {
        _logger.LogInformation("Socket Mode connected");
        return Task.CompletedTask;
    }

    private Task OnSocketModeDisconnected()
    {
        _logger.LogInformation("Socket Mode disconnected");
        return Task.CompletedTask;
    }

    private Task OnSocketModeConnectionError(Exception ex)
    {
        _logger.LogError(ex, "Socket Mode connection error");
        return Task.CompletedTask;
    }

    private async Task<IResult> HandleSlackWebhookAsync(HttpContext context)
    {
        try
        {
            var body = await new StreamReader(context.Request.Body).ReadToEndAsync();
            
            // Verify request signature
            if (!VerifySlackSignature(context.Request.Headers, body))
            {
                _logger.LogWarning("Invalid Slack signature");
                return Results.Unauthorized();
            }

            // Parse the event
            var eventCallback = JsonSerializer.Deserialize<SlackEventCallback>(body);
            if (eventCallback == null)
            {
                _logger.LogWarning("Failed to parse Slack event");
                return Results.BadRequest();
            }

            // Handle URL verification challenge
            if (eventCallback.Type == "url_verification" && !string.IsNullOrEmpty(eventCallback.Challenge))
            {
                _logger.LogInformation("Responding to Slack URL verification challenge");
                return Results.Text(eventCallback.Challenge);
            }

            // Handle message events
            if (eventCallback.Type == "event_callback" && eventCallback.Event.Type == "message")
            {
                await HandleMessageEventAsync(eventCallback.Event);
            }

            return Results.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Slack webhook");
            return Results.StatusCode(500);
        }
    }

    private async Task HandleMessageEventAsync(SlackEvent slackEvent)
    {
        try
        {
            // Skip bot messages and our own messages
            if (!string.IsNullOrEmpty(slackEvent.BotId) || slackEvent.User == _botUserId)
            {
                return;
            }

            // Check if we should respond to this message
            if (!await ShouldRespondToMessageAsync(slackEvent))
            {
                return;
            }

            if (_options.LogMessages)
            {
                _logger.LogInformation("Processing message from user {UserId} in channel {ChannelId}: {Text}",
                    slackEvent.User, slackEvent.Channel, slackEvent.Text);
            }

            // Get user and channel info
            var userInfo = await GetUserInfoAsync(slackEvent.User);
            var channelInfo = await GetChannelInfoAsync(slackEvent.Channel);

            // Create incoming message
            var message = new IncomingMessage
            {
                Content = CleanSlackMessage(slackEvent.Text),
                UserId = slackEvent.User,
                UserName = userInfo?.RealName ?? userInfo?.Name ?? slackEvent.User,
                ChannelId = slackEvent.Channel,
                ChannelName = channelInfo?.Name ?? slackEvent.Channel,
                Channel = ChannelType,
                Type = MessageType.Text,
                Metadata = new Dictionary<string, object>
                {
                    ["slack_timestamp"] = slackEvent.Timestamp,
                    ["slack_event_timestamp"] = slackEvent.EventTimestamp,
                    ["slack_channel_type"] = slackEvent.ChannelType,
                    ["slack_thread_ts"] = slackEvent.ThreadTimestamp ?? string.Empty
                }
            };

            // Process the message
            var response = await ProcessAsync(message);
            
            // Send the response
            if (response.AIResponse != null)
            {
                var context = new MessageContext
                {
                    ChannelId = message.ChannelId,
                    UserId = message.UserId,
                    Channel = message.Channel,
                    OriginalMessage = message
                };

                await SendResponseAsync(response.AIResponse, context);
            }
            else if (!response.Success)
            {
                await SendSlackMessageAsync(slackEvent.Channel, 
                    $"❌ Error: {response.ErrorMessage}", 
                    slackEvent.ThreadTimestamp);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling Slack message event");
        }
    }

    private Task<bool> ShouldRespondToMessageAsync(SlackEvent slackEvent)
    {
        // Check allowed channels
        if (_options.AllowedChannels.Any() && !_options.AllowedChannels.Contains(slackEvent.Channel))
        {
            return Task.FromResult(false);
        }

        // Check allowed users
        if (_options.AllowedUsers.Any() && !_options.AllowedUsers.Contains(slackEvent.User))
        {
            return Task.FromResult(false);
        }

        // Handle direct messages
        if (slackEvent.ChannelType == "im")
        {
            return Task.FromResult(_options.RespondToDirectMessages);
        }

        // Handle channel messages
        if (_options.RespondToMentionsOnly)
        {
            // Check if bot is mentioned
            return Task.FromResult(!string.IsNullOrEmpty(_botUserId) && slackEvent.Text.Contains($"<@{_botUserId}>"));
        }

        return Task.FromResult(true);
    }

    private string CleanSlackMessage(string text)
    {
        // Remove bot mentions
        if (!string.IsNullOrEmpty(_botUserId))
        {
            text = text.Replace($"<@{_botUserId}>", "").Trim();
        }

        // Clean up Slack formatting
        text = text.Replace("&lt;", "<").Replace("&gt;", ">").Replace("&amp;", "&");
        
        return text.Trim();
    }

    private async Task SendSlackMessageAsync(string channel, string text, string? threadTs = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new SlackMessageRequest
            {
                Channel = channel,
                Text = text,
                ThreadTimestamp = (_options.UseThreading && !string.IsNullOrEmpty(threadTs)) ? threadTs : null,
                Username = _options.BotName,
                IconEmoji = _options.BotEmoji
            };

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://slack.com/api/chat.postMessage", content, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var apiResponse = JsonSerializer.Deserialize<SlackApiResponse>(responseBody);
                
                if (apiResponse?.Ok != true)
                {
                    _logger.LogWarning("Slack API error: {Error}", apiResponse?.Error);
                }
            }
            else
            {
                _logger.LogWarning("Failed to send Slack message: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
        }
    }

    private async Task<SlackUser?> GetUserInfoAsync(string userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://slack.com/api/users.info?user={userId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var userResponse = JsonSerializer.Deserialize<SlackUserInfoResponse>(json);
                return userResponse?.User;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get user info for {UserId}", userId);
        }
        return null;
    }

    private async Task<SlackChannel?> GetChannelInfoAsync(string channelId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"https://slack.com/api/conversations.info?channel={channelId}");
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var channelResponse = JsonSerializer.Deserialize<SlackChannelInfoResponse>(json);
                return channelResponse?.Channel;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get channel info for {ChannelId}", channelId);
        }
        return null;
    }

    private async Task<string?> GetBotUserIdAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("https://slack.com/api/auth.test", cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var authResponse = JsonSerializer.Deserialize<JsonElement>(json);
                
                if (authResponse.TryGetProperty("ok", out var ok) && ok.GetBoolean() &&
                    authResponse.TryGetProperty("user_id", out var userId))
                {
                    return userId.GetString();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get bot user ID");
        }
        return null;
    }

    private bool VerifySlackSignature(IHeaderDictionary headers, string body)
    {
        try
        {
            var timestamp = headers["X-Slack-Request-Timestamp"].FirstOrDefault();
            var signature = headers["X-Slack-Signature"].FirstOrDefault();

            if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(signature))
            {
                return false;
            }

            // Check timestamp to prevent replay attacks
            var requestTime = DateTimeOffset.FromUnixTimeSeconds(long.Parse(timestamp));
            if (Math.Abs((DateTimeOffset.UtcNow - requestTime).TotalMinutes) > 5)
            {
                return false;
            }

            // Calculate expected signature
            var baseString = $"v0:{timestamp}:{body}";
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SigningSecret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(baseString));
            var expectedSignature = "v0=" + Convert.ToHexString(hash).ToLower();

            return signature == expectedSignature;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error verifying Slack signature");
            return false;
        }
    }

    private string? GetThreadTimestamp(MessageContext context)
    {
        if (context.OriginalMessage.Metadata.TryGetValue("slack_thread_ts", out var threadTs) && 
            threadTs is string ts && !string.IsNullOrEmpty(ts))
        {
            return ts;
        }
        
        if (context.OriginalMessage.Metadata.TryGetValue("slack_timestamp", out var timestamp) && 
            timestamp is string msgTs)
        {
            return msgTs;
        }
        
        return null;
    }

    public void Dispose()
    {
        _httpClient?.Dispose();
        _cancellationTokenSource?.Dispose();
        _webApp?.DisposeAsync().GetAwaiter().GetResult();
        _socketModeClient?.Dispose();
    }
} 