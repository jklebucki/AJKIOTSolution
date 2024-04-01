using System.ComponentModel.DataAnnotations;

namespace AJKIOT.Shared.Models
{
    public class ResetPasswordConfirmRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Token { get; set; } = string.Empty;
        [Required]
        public string NewPassword { get; set; } = string.Empty;
        [Required]
        public string ApplicationAddress { get; set; } = string.Empty;
    }
}
