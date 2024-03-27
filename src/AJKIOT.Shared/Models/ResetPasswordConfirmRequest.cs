namespace AJKIOT.Shared.Models
{
    public class ResetPasswordConfirmRequest
    {
        public string Email { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
