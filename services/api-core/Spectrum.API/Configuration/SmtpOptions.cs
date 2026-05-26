using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Configuration
{
    public class SmtpOptions
    {
        public const string SectionName = "Smtp";

        [Required]
        public string Host { get; set; } = string.Empty;

        [Range(1, 65535)]
        public int Port { get; set; } = 587;

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string FromEmail { get; set; } = string.Empty;

        [Required]
        public string FromName { get; set; } = "Spectrum";

        public bool UseTls { get; set; } = true;
    }
}
