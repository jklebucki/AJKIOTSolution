using AJKIOT.Api.Services;
using System.Net.WebSockets;
using System.Text;

namespace AJKIOT.Api.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMessageBus _messageBus;
        public WebSocketMiddleware(RequestDelegate next, IMessageBus messageBus)
        {
            _next = next;
            _messageBus = messageBus;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value;
            if (path != null)
            {
                var pathSegments = path.Split('/');
                if (pathSegments.Contains("ws"))
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        await HandleMessage(context, webSocket);
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                    }
                }
                else
                {
                    await _next(context);
                }
            }
            else
            {
                await _next(context);
            }
        }

        private async Task HandleMessage(HttpContext context, WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue)
            {
                var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                // Wysyłanie wiadomości do message bus i oczekiwanie na odpowiedź
                _messageBus.EnqueueMessage(receivedMessage);
                await Task.Delay(100);
                var responseMessage = await _messageBus.GetNextMessageAsync(receivedMessage, "out");
                // Wysłanie odpowiedzi do klienta WebSocket
                var responseBuffer = Encoding.UTF8.GetBytes(responseMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(responseBuffer), result.MessageType, result.EndOfMessage, CancellationToken.None);

                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }
    }
}
