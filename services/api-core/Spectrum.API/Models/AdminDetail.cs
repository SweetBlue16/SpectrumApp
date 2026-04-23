using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents the detailed information of an administrator.
    /// </summary>
    [Table("admin_details")]
    public class AdminDetail
    {
        /// <summary>
        /// Gets or sets the unique identifier for the entity.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the associated user.
        /// </summary>
        [Required]
        [ForeignKey("user_id")]
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// Gets or sets the first name of the administrator.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the last name of the administrator.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the phone number of the administrator.
        /// </summary>
        [Required]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", 
            ErrorMessage = "Phone number must be in a valid format (e.g., +1234567890).")]
        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the address of the administrator.
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column("address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the RFC (Registro Federal de Contribuyentes) of the administrator.
        /// </summary>
        [Required]
        [RegularExpression(@"^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z0-9]{2}[0-9A])?$",
            ErrorMessage = "RFC must be in a valid format (e.g., ABCD123456EFG).")]
        [Column("rfc")]
        public string Rfc { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;
    }
}
