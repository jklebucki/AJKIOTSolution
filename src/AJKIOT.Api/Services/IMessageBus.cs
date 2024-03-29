namespace AJKIOT.Api.Services
{
    public interface IMessageBus
    {
        Task<string> SendMessageAsync(string message);
        void ReceiveMessage(string message);
    }
}
