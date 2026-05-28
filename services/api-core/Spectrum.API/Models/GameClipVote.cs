using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    [Table("game_clip_votes")]
    public class GameClipVote
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Required]
        [Column("clip_id")]
        public Guid ClipId { get; set; }

        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("is_positive")]
        public bool IsPositive { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(ClipId))]
        public virtual GameClip Clip { get; set; } = null!;

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;
    }
}
