using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Represents a cached or locally tracked video game entity within the platform.
    /// </summary>
    [Table("games")]
    public class Game
    {
        /// <summary>
        /// Internal primary key for locally tracked games.
        /// This identifier is independent from the external RAWG numeric game ID.
        /// </summary>
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        /// <summary>
        /// Original numeric identifier provided by the external RAWG API.
        /// Used for data synchronization and preventing duplicates.
        /// </summary>
        [Column("rawg_id")]
        [NotMapped]
        public int RawgId { get; set; }

        /// <summary>
        /// The official title of the video game.
        /// </summary>
        [Required]
        [MaxLength(150)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The primary studio or developer responsible for the game.
        /// </summary>
        [MaxLength(100)]
        [Column("developer")]
        public string Developer { get; set; } = string.Empty;

        /// <summary>
        /// A textual overview of the game.
        /// </summary>
        [Column("description")]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The official launch date.
        /// </summary>
        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }

        /// <summary>
        /// An absolute URI pointing to the game's box art or promotional banner.
        /// </summary>
        [MaxLength(255)]
        [Column("cover_image_url")]
        [JsonPropertyName("background_image")]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// Average rating calculated internally by the Spectrum community.
        /// This is independent of the external RAWG rating.
        /// </summary>
        [Column("internal_rating")]
        [NotMapped]
        public double InternalRating { get; set; } = 0.0;

        /// <summary>
        /// List of genre identifiers associated with the game.
        /// Used for instant filtering in the frontend sidebar.
        /// This property is not mapped to the relational database but persisted in the JSON snapshot.
        /// </summary>
        [NotMapped]
        public List<int> GenreIds { get; set; } = new();

        /// <summary>
        /// List of platform identifiers (e.g., PC, PS5) associated with the game.
        /// Used for instant filtering in the frontend sidebar.
        /// This property is not mapped to the relational database but persisted in the JSON snapshot.
        /// </summary>
        [NotMapped]
        public List<int> PlatformIds { get; set; } = new();

        /// <summary>
        /// Navigation property for users that have the game marked like interest. 
        /// </summary>
        public virtual ICollection<User> InterestedUsers { get; set; } = new List<User>();
    }
}