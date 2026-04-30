using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents a critical evaluation submitted by a user for a specific game. 
    /// This entity forms the core of the user-generated content engine, serving as the 
    /// aggregate root for subsequent social interactions (like votes and comments) 
    /// that are managed in external NoSQL microservices.
    /// </summary>
    [Table("reviews")]
    public class Review
    {
        /// <summary>
        /// The primary key generated as a universally unique identifier (UUID/GUID). 
        /// Acts as the authoritative reference target for linking social interactions 
        /// (comments, upvotes, reports) stored across distributed microservices.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The foreign key linking this review to its author. Essential for querying 
        /// a user's activity history and enforcing authorization boundaries (e.g., 
        /// ensuring only the author or an admin can modify or delete the review).
        /// </summary>
        [Required]
        [Column("user_id")]
        public Guid UserId { get; set; }

        /// <summary>
        /// The foreign key linking this review to the internal game catalog. 
        /// Enables the aggregation of average ratings and efficient retrieval 
        /// of all public reviews for a specific title.
        /// </summary>
        [Required]
        [Column("game_id")]
        public Guid GameId { get; set; }

        /// <summary>
        /// A quantifiable evaluation metric strictly bounded to a discrete scale (1 to 5). 
        /// This constraint guarantees data integrity when calculating the aggregate 
        /// score of a game across the platform.
        /// </summary>
        [Required]
        [Range(1, 5)]
        [Column("rating")]
        public int Rating { get; set; }

        /// <summary>
        /// The primary textual body containing the user's critique. Expected to be 
        /// validated against content length constraints and sanitized at the application 
        /// layer to prevent injection attacks before persistence.
        /// </summary>
        [Required]
        [Column("content")]
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// An optional URI pointing to a user-uploaded image hosted on external object storage 
        /// (e.g., AWS S3, Cloudinary). Constrained to 255 characters to optimize database row size.
        /// </summary>
        [MaxLength(255)]
        [Column("image_url")]
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// Implements the "soft-delete" pattern. Allows the platform to hide the review 
        /// from public endpoints while preserving the data for moderation audits, thereby 
        /// avoiding the risk of cascading deletes breaking associated NoSQL social data.
        /// </summary>
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// The UTC timestamp marking the exact moment the review was published. 
        /// Used for sorting activity feeds chronologically and managing cache invalidation.
        /// </summary>
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Entity Framework navigation property establishing a many-to-one relationship 
        /// with the <see cref="Models.User"/> entity. Facilitates eager or explicit loading 
        /// of the author's profile details.
        /// </summary>
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        /// <summary>
        /// Entity Framework navigation property establishing a many-to-one relationship 
        /// with the <see cref="Models.Game"/> entity. Essential for navigating from a 
        /// user's review back to the game's metadata.
        /// </summary>
        [ForeignKey("GameId")]
        public virtual Game Game { get; set; } = null!;
    }
}
