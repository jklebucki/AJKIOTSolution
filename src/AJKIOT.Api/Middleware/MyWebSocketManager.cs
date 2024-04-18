using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace AJKIOT.Api.Middleware
{
    public class MyWebSocketManager : IWebSocketManager
    {
        private ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public void AddSocket(string clientId, WebSocket socket)
        {
            _sockets.TryAdd(clientId, socket);
        }

        public async Task RemoveSocket(string clientId)
        {
            if (_sockets.TryRemove(clientId, out WebSocket? socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "The connection is closed by the server", CancellationToken.None);
            }
        }

        public async Task SendMessageToClientAsync(string clientId, string message)
        {
            if (_sockets.TryGetValue(clientId, out WebSocket? socket))
            {
                if (socket.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }

        public async Task BroadcastMessageAsync(string message)
        {
            foreach (var pair in _sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    var buffer = Encoding.UTF8.GetBytes(message);
                    await pair.Value.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
        }
    }
}
