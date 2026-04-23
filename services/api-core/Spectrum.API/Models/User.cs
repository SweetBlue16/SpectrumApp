using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents a system user, either a Reviewer or an Administrator.
    /// </summary>
    [Table("users")]
    public class User
    {
        /// <summary>
        /// Unique identifier for the user.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Public display name and unique identifier for mentions.
        /// </summary>
        [Required]
        [MaxLength(50)]
        [Column("username")]
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Primary email address used for communication and login.
        /// </summary>
        [Required]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Secure BCrypt-hashed password string.
        /// </summary>
        [Required]
        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// The security role assigned to the user (e.g., Admin, Reviewer).
        /// </summary>
        [Required]
        [MaxLength(20)]
        [Column("role")]
        public string Role { get; set; } = UserRole.Reviewer;

        /// <summary>
        /// Flag indicating if the user is currently banned from performing actions.
        /// </summary>
        [Column("is_suspended")]
        public bool IsSuspended { get; set; } = false;

        /// <summary>
        /// Gets or sets a value indicating whether the entity is marked as deleted.
        /// </summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of the account creation in UTC.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property for the associated admin details, if the user is an administrator.
        /// </summary>
        public virtual AdminDetail? AdminDetail { get; set; }
    }

    public static class UserRole
    {
        public const string Admin = "ADMIN";
        public const string Reviewer = "REVIEWER";
    }
}
