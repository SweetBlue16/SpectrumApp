using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    /// <summary>
    /// Entity representing a video game stored in the local database for reviews and tracking.
    /// </summary>
    [Table("games")]
    public class Game
    {
        /// <summary>
        /// Local unique identifier for the game record.
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

        [MaxLength(100)]
        [Column("developer")]
        public string Developer { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("release_date")]
        public DateTime? ReleaseDate { get; set; }

        [MaxLength(255)]
        [Column("cover_image_url")]
        public string? CoverImageUrl { get; set; }

        /// <summary>
        /// Collection of user reviews associated with this game.
        /// </summary>
        public ICollection<Review> Reviews { get; set; }
    }
}
