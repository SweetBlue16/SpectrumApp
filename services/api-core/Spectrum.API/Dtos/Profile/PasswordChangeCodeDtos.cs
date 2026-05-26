using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Profile
{
    public class VerifyPasswordChangeCodeDto
    {
        [Required]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must contain 6 digits.")]
        public string Code { get; set; } = string.Empty;
    }

    public class ConfirmPasswordChangeDto
    {
        [Required]
        public string VerificationToken { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain one uppercase letter, one lowercase letter, and one number.")]
        public string NewPassword { get; set; } = string.Empty;
    }
}
