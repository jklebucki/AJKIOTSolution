using AJKIOT.Api.Services;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace AJKIOT.Api.Middleware
{
    public class MyWebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<MyWebSocketMiddleware> _logger;
        private IWebSocketManager _webSocketManager;
        private readonly int _bufferSize;

        public MyWebSocketMiddleware(RequestDelegate next, IMessageBus messageBus, ILogger<MyWebSocketMiddleware> logger, IWebSocketManager webSocketManager, int bufferSize = 16384)
        {
            _next = next;
            _messageBus = messageBus;
            _logger = logger;
            _webSocketManager = webSocketManager;
            _bufferSize = bufferSize;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    string clientId = await ReceiveInitialClientIdAsync(webSocket);
                    if (!string.IsNullOrEmpty(clientId))
                    {
                        _webSocketManager.AddSocket(clientId, webSocket);
                        await HandleWebSocketCommunication(clientId, webSocket, context.RequestAborted);
                    }
                    else
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Invalid clientId", CancellationToken.None);
                    }
                }
                else
                {
                    _logger.LogWarning("Non-WebSocket request received");
                    context.Response.StatusCode = 400;
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task<string> ReceiveInitialClientIdAsync(WebSocket webSocket)
        {
            var buffer = new byte[_bufferSize];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string messageJson = Encoding.UTF8.GetString(buffer, 0, result.Count);
                try
                {
                    var message = JsonDocument.Parse(messageJson).RootElement;
                    var id = message.GetProperty("_id").GetString();
                    return id;
                }
                catch (JsonException ex)
                {
                    _logger.LogError($"Error deserializing clientId: {ex}");
                }
            }
            return null;
        }

        private async Task HandleWebSocketCommunication(string clientId, WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[_bufferSize];
            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    }
                    else
                    {
                        _logger.LogInformation($"Received message from client {clientId}");
                        await ProcessMessageAsync(webSocket, buffer, result, cancellationToken);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during WebSocket communication.");
            }
            finally
            {
                _ = _webSocketManager.RemoveSocket(clientId);
            }
        }

        private async Task ProcessMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketReceiveResult result, CancellationToken cancellationToken)
        {
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation("Received message: {Message}", receivedMessage);
            _messageBus.EnqueueMessage(receivedMessage);
            await Task.FromResult(string.Empty);
        }
    }
}
