using AJKIOT.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AJKIOT.Api.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMessageBus _messageBus;
        private readonly ILogger<WebSocketMiddleware> _logger;
        private readonly int _bufferSize;

        public WebSocketMiddleware(RequestDelegate next, IMessageBus messageBus, ILogger<WebSocketMiddleware> logger, int bufferSize = 16384)
        {
            _next = next;
            _messageBus = messageBus;
            _logger = logger;
            _bufferSize = bufferSize;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value!;
            var pathSegments = path.Split('/');
            if (pathSegments.Contains("ws") && context.WebSockets.IsWebSocketRequest)
            {
                _logger.LogInformation("WebSocket connection starting.");

                using (WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync())
                {
                    await HandleWebSocketCommunication(context, webSocket, context.RequestAborted);
                }

                _logger.LogInformation("WebSocket connection closed.");
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleWebSocketCommunication(HttpContext context, WebSocket webSocket, CancellationToken cancellationToken)
        {
            var buffer = new byte[_bufferSize];
            WebSocketReceiveResult result = null!;

            try
            {
                do
                {
                    result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket connection closing; CloseStatus: {CloseStatus}, CloseStatusDescription: {CloseStatusDescription}", result.CloseStatus, result.CloseStatusDescription);
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, cancellationToken);
                    }
                    else
                    {
                        await ProcessMessageAsync(webSocket, buffer, result, cancellationToken);
                    }
                }
                while (!result.CloseStatus.HasValue);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred during WebSocket communication.");
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal server error", cancellationToken);
                }
            }
        }

        private async Task ProcessMessageAsync(WebSocket webSocket, byte[] buffer, WebSocketReceiveResult result, CancellationToken cancellationToken)
        {
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            _logger.LogInformation("Received message: {Message}", receivedMessage);

            // Sending message to the message bus and waiting for a response
            _messageBus.EnqueueMessage(receivedMessage);
            await Task.Delay(50);
            var responseMessage = await _messageBus.GetNextMessageAsync(receivedMessage, "out");

            // Sending response back to the WebSocket client
            var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), result.MessageType, true, cancellationToken);
            _logger.LogInformation("Sent response: {Response}", responseMessage);
        }
    }
}
