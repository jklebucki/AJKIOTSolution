namespace AJKIOT.Shared.Models
{
    public class ResetPasswordConfirmRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
        public string ApplicationAddress { get; set; } = string.Empty;
    }
}
