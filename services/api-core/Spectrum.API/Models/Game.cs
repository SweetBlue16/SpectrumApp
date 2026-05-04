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
        public string? CoverImageUrl { get; set; }
    }
}