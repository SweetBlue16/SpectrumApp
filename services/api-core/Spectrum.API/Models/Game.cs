using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    [Table("games")]
    public class Game
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

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

        public ICollection<Review> Reviews { get; set; }
    }
}
