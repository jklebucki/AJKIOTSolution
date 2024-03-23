using System.ComponentModel.DataAnnotations;

namespace AJKIOT.Web.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    sealed public class StrongPasswordAttribute : ValidationAttribute
    {
        public int MinimumLength { get; set; } = 8;

        public StrongPasswordAttribute() : base("The password is not strong enough.")
        {
        }

        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {

            var message = $"The password must be at least {MinimumLength} characters long, one uppercase letter, one lowercase letter, one number and one special character.";

            if (value == null || string.IsNullOrEmpty(value.ToString()))
            {
                return new ValidationResult("Password is required.");
            }

            var password = value.ToString();

            if (password != null && (password.Length < MinimumLength || !password.Any(char.IsUpper)
                || !password.Any(char.IsLower) || !password.Any(char.IsDigit) || password.All(char.IsLetterOrDigit)))
            {
                return new ValidationResult(message);
            }

            return ValidationResult.Success!;

        }
    }
}
