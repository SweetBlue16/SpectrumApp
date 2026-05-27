using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    [Table("user_blocks")]
    public class UserBlock
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [Column("blocker_user_id")]
        public Guid BlockerUserId { get; set; }

        [Required]
        [Column("blocked_user_id")]
        public Guid BlockedUserId { get; set; }

        [MaxLength(200)]
        [Column("reason")]
        public string? Reason { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
