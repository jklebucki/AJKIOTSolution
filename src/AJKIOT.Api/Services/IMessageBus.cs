using System.Text.Json;

namespace AJKIOT.Api.Services
{
    public interface IMessageBus
    {
        void EnqueueMessage(string message);
        Task<string> GetNextIncomingMessageAsync();
        Task<string> GetNextMessageAsync(string message, string type);
    }
}
