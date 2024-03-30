using System.Net.WebSockets;

namespace AJKIOT.Api.Middleware
{
    public interface IWebSocketManager
    {
        void AddSocket(string clientId, WebSocket socket);
        Task RemoveSocket(string clientId);
        Task SendMessageToClientAsync(string clientId, string message);
        Task BroadcastMessageAsync(string message);
    }
}
