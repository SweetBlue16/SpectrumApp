using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents a video clip uploaded by a user for a specific video game.
    /// </summary>
    [Table("game_clips")]
    public class GameClip
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Url]
        public string Url { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid GameId { get; set; }

        /// <summary>
        /// Timestamp of when the clip was successfully uploaded and registered.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by_user_id")]
        public Guid? DeletedByUserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; } = null!;

        [ForeignKey(nameof(GameId))]
        public virtual Game Game { get; set; } = null!;

        public virtual ICollection<GameClipVote> Votes { get; set; } = new List<GameClipVote>();
    }
}
