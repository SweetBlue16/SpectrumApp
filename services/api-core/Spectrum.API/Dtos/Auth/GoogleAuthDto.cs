using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Auth
{
    /// <summary>
    /// Data transfer object for handling Google Single Sign-On (SSO) authentication requests.
    /// </summary>
    public class GoogleAuthDto
    {
        /// <summary>
        /// The identity token (JWT) credential issued directly by the Google Client SDK.
        /// </summary>
        [Required(ErrorMessage = "The Google credential token is required.")]
        public string Credential { get; set; } = string.Empty;
    }
}
