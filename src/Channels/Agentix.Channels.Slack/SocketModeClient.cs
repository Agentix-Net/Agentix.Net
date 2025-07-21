using Agentix.Channels.Slack.Models;
using Microsoft.Extensions.Logging;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Agentix.Channels.Slack;

/// <summary>
/// Internal WebSocket client for Slack Socket Mode connections.
/// Handles WebSocket communication, reconnection logic, and message processing.
/// </summary>
internal class SocketModeClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SocketModeClient> _logger;
    private readonly SlackChannelOptions _options;
    private ClientWebSocket? _webSocket;
    private CancellationTokenSource? _cancellationTokenSource;
    private Task? _receiveTask;
    private Task? _heartbeatTask;
    private int _pingId = 0;
    private readonly SemaphoreSlim _connectionSemaphore = new(1, 1);
    private bool _isConnected = false;
    private int _reconnectAttempts = 0;

    public event Func<SocketModeEnvelope, Task>? MessageReceived;
    public event Func<Exception, Task>? ConnectionError;
    public event Func<Task>? Connected;
    public event Func<Task>? Disconnected;

    public bool IsConnected => _isConnected && _webSocket?.State == WebSocketState.Open;

    public SocketModeClient(HttpClient httpClient, ILogger<SocketModeClient> logger, SlackChannelOptions options)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options;
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (IsConnected)
            {
                _logger.LogWarning("Socket Mode client is already connected");
                return;
            }

            // Get WebSocket URL from Slack
            var socketUrl = await GetSocketUrlAsync(cancellationToken);
            if (string.IsNullOrEmpty(socketUrl))
            {
                throw new InvalidOperationException("Failed to get Socket Mode URL from Slack");
            }

            if (_options.LogConnectionEvents)
            {
                _logger.LogInformation("Connecting to Slack Socket Mode: {Url}", socketUrl);
            }

            // Create and connect WebSocket
            _webSocket = new ClientWebSocket();
            _cancellationTokenSource = new CancellationTokenSource();

            await _webSocket.ConnectAsync(new Uri(socketUrl), cancellationToken);

            _isConnected = true;
            _reconnectAttempts = 0;

            // Start receiving messages
            _receiveTask = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token), cancellationToken);

            // Start heartbeat
            _heartbeatTask = Task.Run(() => HeartbeatAsync(_cancellationTokenSource.Token), cancellationToken);

            if (_options.LogConnectionEvents)
            {
                _logger.LogInformation("Successfully connected to Slack Socket Mode");
            }

            await (Connected?.Invoke() ?? Task.CompletedTask);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _connectionSemaphore.WaitAsync(cancellationToken);
        try
        {
            if (!_isConnected)
            {
                return;
            }

            _isConnected = false;
            _cancellationTokenSource?.Cancel();

            if (_webSocket?.State == WebSocketState.Open)
            {
                await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", cancellationToken);
            }

            // Wait for tasks to complete
            if (_receiveTask != null)
            {
                try { await _receiveTask; } catch (OperationCanceledException) { }
            }
            if (_heartbeatTask != null)
            {
                try { await _heartbeatTask; } catch (OperationCanceledException) { }
            }

            _webSocket?.Dispose();
            _webSocket = null;
            _cancellationTokenSource?.Dispose();
            _cancellationTokenSource = null;

            if (_options.LogConnectionEvents)
            {
                _logger.LogInformation("Disconnected from Slack Socket Mode");
            }

            await (Disconnected?.Invoke() ?? Task.CompletedTask);
        }
        finally
        {
            _connectionSemaphore.Release();
        }
    }

    public async Task SendAcknowledgmentAsync(string envelopeId, object? payload = null)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("Cannot send acknowledgment - not connected");
            return;
        }

        var ack = new SocketModeAck
        {
            EnvelopeId = envelopeId,
            Payload = payload
        };

        var json = JsonSerializer.Serialize(ack);
        await SendMessageAsync(json);
    }

    private async Task<string?> GetSocketUrlAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "https://slack.com/api/apps.connections.open");
            request.Headers.Add("Authorization", $"Bearer {_options.AppToken}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                var connectionResponse = JsonSerializer.Deserialize<SocketModeConnectionResponse>(json);
                
                if (connectionResponse?.Ok == true)
                {
                    return connectionResponse.Url;
                }
                else
                {
                    _logger.LogError("Slack apps.connections.open failed: {Error}", connectionResponse?.Error);
                }
            }
            else
            {
                _logger.LogError("HTTP error getting Socket Mode URL: {StatusCode}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Socket Mode URL");
        }

        return null;
    }

    private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
    {
        var buffer = new byte[4096];
        var messageBuilder = new StringBuilder();

        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                var result = await _webSocket!.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var text = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    messageBuilder.Append(text);

                    if (result.EndOfMessage)
                    {
                        var message = messageBuilder.ToString();
                        messageBuilder.Clear();

                        await ProcessMessageAsync(message);
                    }
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    _logger.LogInformation("WebSocket closed by server: {Status} {Description}", 
                        result.CloseStatus, result.CloseStatusDescription);
                    break;
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error receiving WebSocket messages");
            await HandleConnectionError(ex);
        }
    }

    private async Task ProcessMessageAsync(string message)
    {
        try
        {
            if (_options.LogConnectionEvents)
            {
                _logger.LogDebug("Received Socket Mode message: {Message}", message);
            }

            var envelope = JsonSerializer.Deserialize<SocketModeEnvelope>(message);
            if (envelope == null)
            {
                _logger.LogWarning("Failed to deserialize Socket Mode message");
                return;
            }

            // Handle different message types
            switch (envelope.Type)
            {
                case "hello":
                    _logger.LogDebug("Received hello message from Slack");
                    break;

                case "disconnect":
                    _logger.LogInformation("Received disconnect message from Slack");
                    await DisconnectAsync();
                    break;

                case "events_api":
                    // Send acknowledgment first
                    await SendAcknowledgmentAsync(envelope.EnvelopeId);
                    
                    // Then process the event
                    if (MessageReceived != null)
                    {
                        await MessageReceived.Invoke(envelope);
                    }
                    break;

                case "interactive":
                case "slash_commands":
                case "options":
                    // Send acknowledgment and process
                    await SendAcknowledgmentAsync(envelope.EnvelopeId);
                    
                    if (MessageReceived != null)
                    {
                        await MessageReceived.Invoke(envelope);
                    }
                    break;

                default:
                    _logger.LogDebug("Received unknown Socket Mode message type: {Type}", envelope.Type);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Socket Mode message: {Message}", message);
        }
    }

    private async Task HeartbeatAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                await Task.Delay(_options.HeartbeatInterval, cancellationToken);

                if (IsConnected)
                {
                    var ping = new SocketModePing { Id = ++_pingId };
                    var json = JsonSerializer.Serialize(ping);
                    await SendMessageAsync(json);

                    if (_options.LogConnectionEvents)
                    {
                        _logger.LogDebug("Sent heartbeat ping: {Id}", ping.Id);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancelling
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in heartbeat loop");
            await HandleConnectionError(ex);
        }
    }

    private async Task SendMessageAsync(string message)
    {
        if (!IsConnected)
        {
            return;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await _webSocket!.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, CancellationToken.None);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WebSocket message");
            await HandleConnectionError(ex);
        }
    }

    private async Task HandleConnectionError(Exception ex)
    {
        _isConnected = false;

        await (ConnectionError?.Invoke(ex) ?? Task.CompletedTask);

        if (_options.AutoReconnect && _reconnectAttempts < _options.MaxReconnectAttempts)
        {
            _reconnectAttempts++;
            _logger.LogInformation("Attempting to reconnect ({Attempt}/{MaxAttempts})...", 
                _reconnectAttempts, _options.MaxReconnectAttempts);

            try
            {
                await Task.Delay(_options.ReconnectTimeout);
                await ConnectAsync();
            }
            catch (Exception reconnectEx)
            {
                _logger.LogError(reconnectEx, "Reconnection attempt failed");
            }
        }
        else
        {
            _logger.LogError("Max reconnection attempts reached or auto-reconnect disabled");
        }
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
        _connectionSemaphore?.Dispose();
    }
} 