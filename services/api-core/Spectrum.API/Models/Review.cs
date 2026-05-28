using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents a critical evaluation submitted by a user for a specific external game.
    /// The game identifier corresponds to the RAWG catalog numeric ID consumed by Spectrum.
    /// </summary>
    [Table("reviews")]
    public class Review
    {
        /// <summary>
        /// Internal review identifier used as the authoritative target for votes,
        /// comments and moderation actions.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Author of the review.
        /// </summary>
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// External RAWG game identifier. Example: 58781.
        /// </summary>
        [Required]
        [Column("game_id")]
        public int GameId { get; set; }

        /// <summary>
        /// User rating from 5 to 10.
        /// </summary>
        [Required]
        [Range(5, 10)]
        [Column("rating")]
        public int Rating { get; set; }

        /// <summary>
        /// Short public title for the review.
        /// </summary>
        [Required]
        [MinLength(1)]
        [MaxLength(120)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Textual content of the review.
        /// </summary>
        [Required]
        [MinLength(1)]
        [MaxLength(2000)]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Optional image URL associated with the review.
        /// </summary>
        [MaxLength(255)]
        [Column("image_url")]
        public string? ImageUrl { get; set; }

        /// <summary>
        /// Optional media MIME type for the review attachment.
        /// </summary>
        [MaxLength(50)]
        [Column("media_type")]
        public string? MediaType { get; set; }

        /// <summary>
        /// Cached likes count returned from the social/voting subsystem when applicable.
        /// </summary>
        [Column("likes_count")]
        public int LikesCount { get; set; }

        /// <summary>
        /// Cached dislikes count returned from the social/voting subsystem when applicable.
        /// </summary>
        [Column("dislikes_count")]
        public int DislikesCount { get; set; }

        /// <summary>
        /// Soft-delete flag for moderation and auditability.
        /// </summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; }

        /// <summary>
        /// Creation timestamp in UTC.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Last update timestamp in UTC.
        /// </summary>
        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted_at")]
        public DateTime? DeletedAt { get; set; }

        [Column("deleted_by_admin_id")]
        public Guid? DeletedByAdminId { get; set; }

        [MaxLength(300)]
        [Column("deletion_reason")]
        public string? DeletionReason { get; set; }

        /// <summary>
        /// Review author navigation property.
        /// </summary>
        [ForeignKey(nameof(UserId))]
        public virtual User? User { get; set; }
    }
}
