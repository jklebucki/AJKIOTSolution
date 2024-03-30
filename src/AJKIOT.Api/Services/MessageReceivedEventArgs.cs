namespace AJKIOT.Api.Services
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public string Id { get; }
        public string Message { get; }

        public MessageReceivedEventArgs(string id, string message)
        {
            Id = id;
            Message = message;
        }
    }
}
