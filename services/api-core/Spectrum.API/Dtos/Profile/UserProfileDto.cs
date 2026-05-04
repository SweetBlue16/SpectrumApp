using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Profile
{
    /// <summary>
    /// Data transfer object representing the detailed profile information of a user.
    /// </summary>
    public class UserProfileDto
    {
        /// <summary>
        /// The authenticated user's public display name.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The authenticated user's primary email address.
        /// </summary>
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The URL or filename of the user's profile image. This can be null if the user has not set a custom photo.
        /// </summary>
        public string? ProfilePicture { get; set; }
    }
}