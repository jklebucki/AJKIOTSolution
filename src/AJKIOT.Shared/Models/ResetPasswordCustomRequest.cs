using System.ComponentModel.DataAnnotations;

namespace AJKIOT.Shared.Models
{
    public class ResetPasswordCustomRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string ApplicationAddress { get; set; } = string.Empty;
    }
}
