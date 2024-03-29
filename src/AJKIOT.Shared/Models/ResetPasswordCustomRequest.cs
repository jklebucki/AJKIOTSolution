namespace AJKIOT.Shared.Models
{
    public class ResetPasswordCustomRequest
    {
        public string Email { get; set; } = string.Empty;
        public string ApplicationAddress { get; set; } = string.Empty;
    }
}
