using System.Numerics;

namespace AJKIOT.Shared.Models
{
    public class Email
    {
        public string FromName { get; set; } = string.Empty;
        public string FromEmail { get; set; } = string.Empty;
        public string ToName { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsHtml { get; set; }
    }
}
