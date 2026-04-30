using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents a cached or locally tracked video game entity within the platform. 
    /// While external services (e.g., RAWG) provide extensive metadata, this entity acts as the 
    /// local aggregate root. It ensures referential integrity for user-generated content 
    /// (such as reviews and drop events) without depending on external catalog uptime.
    /// </summary>
    [Table("games")]
    public class Game
    {
        /// <summary>
        /// The primary key generated as a universally unique identifier (UUID/GUID). 
        /// Serves as the internal relational anchor for linking reviews, abstracting away 
        /// the underlying external catalog's integer-based ID system.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// The official title of the video game. Constrained to 150 characters to ensure 
        /// optimal indexing for local search queries and UI layout consistency.
        /// </summary>
        [Required]
        [MaxLength(150)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The primary studio or developer responsible for the game. Stored locally to allow 
        /// basic filtering and categorization without requiring external API calls.
        /// </summary>
        [MaxLength(100)]
        [Column("developer")]
        public string Developer { get; set; } = string.Empty;

        /// <summary>
        /// A textual overview of the game. Typically populated from the external catalog 
        /// upon first synchronization and cached to reduce external API rate limiting and latency.
        /// </summary>
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The official launch date. Nullable to accommodate upcoming titles, early access, 
        /// or games with unannounced release schedules.
        /// </summary>
        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// An absolute URI pointing to the game's box art or promotional banner. 
        /// Usually points to an external CDN (like RAWG media) to offload bandwidth costs 
        /// from the local infrastructure. Constrained to 255 characters.
        /// </summary>
        [MaxLength(255)]
        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// Entity Framework navigation property establishing a one-to-many relationship 
        /// with the <see cref="Models.Review"/> entity. Facilitates the aggregation and 
        /// eager loading of all community feedback associated with this specific title.
        /// </summary>
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
