using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Auth
{
    /// <summary>
    /// Data transfer object containing the required fields to provision a new standard user account.
    /// </summary>
    public class RegisterDto
    {
        /// <summary>
        /// The desired unique username for the new account.
        /// </summary>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9_]+$",
            ErrorMessage = "Username can only contain letters, numbers, and underscores.")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The primary email address for the new account.
        /// </summary>
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The secure password for the new account. Must meet strict complexity requirements.
        /// </summary>
        [Required(ErrorMessage = "Password is required.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long and contain at least " +
            "one uppercase letter, one lowercase letter, and one number.")]
        public string Password { get; set; } = string.Empty;
    }
}
