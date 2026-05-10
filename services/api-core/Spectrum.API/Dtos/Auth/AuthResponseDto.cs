using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Auth
{
    /// <summary>
    /// Data transfer object representing the payload returned upon a successful authentication or registration.
    /// </summary>
    public class AuthResponseDto
    {
        /// <summary>
        /// The stateless JSON Web Token (JWT) used for authorizing subsequent requests.
        /// </summary>
        [Required]
        public string Token { get; set; } = string.Empty;

        /// <summary>
        /// The authenticated user's public display name.
        /// </summary>
        [Required]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// The authenticated user's primary email address.
        /// </summary>
        [Required]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// The authenticated user's role within the application.
        /// </summary>
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
