using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents the extended, highly sensitive personal details of an administrator.
    /// This entity utilizes the Table Splitting pattern to segregate Personally Identifiable 
    /// Information (PII) from the primary <see cref="Models.User"/> table. This ensures the base 
    /// user table remains lightweight and minimizes the exposure of legal and contact data.
    /// </summary>
    [Table("admin_details")]
    public class AdminDetail
    {
        /// <summary>
        /// The primary key generated as a universally unique identifier (UUID/GUID).
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The foreign key linking this sensitive profile to the base identity record. 
        /// Enforces a strict one-to-one relationship ensuring that only designated personnel 
        /// possess these extended attributes.
        /// </summary>
        [Required]
        [ForeignKey("user_id")]
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The legal first name(s) of the administrator. Constrained to 50 characters 
        /// to standardize reporting and interface rendering.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("first_name")]
        public string FirstName { get; set; } = string.Empty;

        /// <summary>
        /// The legal last name(s) of the administrator. Constrained to 50 characters 
        /// to standardize reporting and interface rendering.
        /// </summary>
        [Required]
        [StringLength(50)]
        [Column("last_name")]
        public string LastName { get; set; } = string.Empty;

        /// <summary>
        /// A verified contact number used for critical system alerts or potential Multi-Factor 
        /// Authentication (MFA). Validated against the E.164 international telecommunication standard.
        /// </summary>
        [Required]
        [RegularExpression(@"^\+?[1-9]\d{1,14}$", 
            ErrorMessage = "Phone number must be in a valid format (e.g., +1234567890).")]
        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        /// <summary>
        /// The physical or billing address of the administrator, required for internal 
        /// auditing, compliance, or payroll purposes.
        /// </summary>
        [Required]
        [StringLength(255)]
        [Column("address")]
        public string Address { get; set; } = string.Empty;

        /// <summary>
        /// The Mexican Tax ID (Registro Federal de Contribuyentes). Essential for legal 
        /// identity verification, tax compliance, and formal internal accounting. 
        /// Validated rigorously to prevent malformed data entry.
        /// </summary>
        [Required]
        [RegularExpression(@"^([A-ZÑ&]{3,4}) ?(?:- ?)?(\d{2}(?:0[1-9]|1[0-2])(?:0[1-9]|[12]\d|3[01])) ?(?:- ?)?([A-Z0-9]{2}[0-9A])?$",
            ErrorMessage = "RFC must be in a valid format (e.g., ABCD123456EFG).")]
        [Column("rfc")]
        public string Rfc { get; set; } = string.Empty;

        /// <summary>
        /// Entity Framework navigation property establishing the inverse of the one-to-one 
        /// relationship with the <see cref="Models.User"/> entity.
        /// </summary>
        public virtual User User { get; set; } = null!;
    }
}
