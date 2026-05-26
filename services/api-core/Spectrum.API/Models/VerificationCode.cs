using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    [Table("verification_codes")]
    public class VerificationCode
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("user_id")]
        public Guid? UserId { get; set; }

        [Required]
        [Column("purpose")]
        public VerificationPurpose Purpose { get; set; }

        [Required]
        [Column("code_hash")]
        public string CodeHash { get; set; } = string.Empty;

        [Column("session_token_hash")]
        public string? SessionTokenHash { get; set; }

        [Column("attempts")]
        public int Attempts { get; set; }

        [Column("max_attempts")]
        public int MaxAttempts { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        [Column("expires_at")]
        public DateTime ExpiresAt { get; set; }

        [Column("verified_at")]
        public DateTime? VerifiedAt { get; set; }

        [Column("used_at")]
        public DateTime? UsedAt { get; set; }

        public virtual User? User { get; set; }
    }
}
