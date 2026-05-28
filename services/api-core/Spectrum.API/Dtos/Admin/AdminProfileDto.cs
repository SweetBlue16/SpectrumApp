using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Admin
{
    public class AdminProfileDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Rfc { get; set; } = string.Empty;
        public string? ProfilePicture { get; set; }
        public string Role { get; set; } = string.Empty;
    }

    public class UpdateAdminProfileDto
    {
        [Required]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(255, MinimumLength = 5)]
        public string Address { get; set; } = string.Empty;

        [StringLength(2048)]
        public string? ProfilePicture { get; set; }
    }
}
