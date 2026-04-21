using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Entity representing a critical review or user opinion about a specific game.
    /// </summary>
    [Table("reviews")]
    public class Review
    {
        /// <summary>
        /// Unique identifier for the review.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Foreign key identifying the author of the review.
        /// </summary>
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [Column("game_id")]
        public Guid GameId { get; set; }

        /// <summary>
        /// Numerical rating given to the game (e.g., 1 to 5 stars).
        /// </summary>
        [Required]
        [Range(1, 5)]
        [Column("rating")]
        public int Rating { get; set; }

        /// <summary>
        /// Descriptive text or body of the review.
        /// </summary>
        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        [MaxLength(255)]
        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Timestamp of when the review was initially posted.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Navigation property to the user who wrote the review.
        /// </summary>
        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("GameId")]
        public Game Game { get; set; }
    }
}
