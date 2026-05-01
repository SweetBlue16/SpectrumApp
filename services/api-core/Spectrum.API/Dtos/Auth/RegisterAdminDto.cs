using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Auth
{
    /// <summary>
    /// Data transfer object extending the standard registration payload with highly sensitive 
    /// personal details and security keys required to provision an Administrator account.
    /// </summary>
    public class RegisterAdminDto : RegisterDto
    {
        /// <summary>
        /// The legal first name(s) of the administrator.
        /// </summary>
        [Required(ErrorMessage = "First name is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "First name must be between 2 and 50 characters.")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The legal last name(s) of the administrator.
        /// </summary>
        [Required(ErrorMessage = "Last name is required.")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessage = "Last name must be between 2 and 50 characters.")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// A verified contact number for the administrator (E.164 standard format).
        /// </summary>
        [Required(ErrorMessage = "Phone number is required.")]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$",
            ErrorMessage = "Invalid phone number format. It should be in E.164 format.")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// The physical or billing address of the administrator.
        /// </summary>
        [Required(ErrorMessage = "Address is required.")]
        [StringLength(255, MinimumLength = 5,
            ErrorMessage = "Address must be between 5 and 255 characters.")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// The Mexican Tax ID (Registro Federal de Contribuyentes) of the administrator.
        /// </summary>
        [Required(ErrorMessage = "RFC is required.")]
        [RegularExpression(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$",
            ErrorMessage = "Invalid RFC format. It should be 3 or 4 uppercase letters " +
            "followed by 6 digits and 3 alphanumeric characters.")]
        public string Rfc { get; set; } = string.Empty;

        /// <summary>
        /// The system-level master key required to authorize the creation of an admin role. 
        /// Prevents standard users from escalating privileges.
        /// </summary>
        [Required(ErrorMessage = "Admin secret key is required.")]
        public string AdminSecretKey { get; set; } = string.Empty;
    }
}
