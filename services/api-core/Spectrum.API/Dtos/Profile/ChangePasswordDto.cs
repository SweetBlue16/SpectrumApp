using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Profile
{
    /// <summary>
    /// Data transfer object for secure password change operations.
    /// </summary>
    public class ChangePasswordDto
    {
        [Required]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; } = string.Empty;
    }
}