using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Spectrum.API.Models
{
    [Table("platforms")]
    public class Platform
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        [Column("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Navegation property for the users that plays in this platform.
        /// </summary>
        public virtual ICollection<User> InterestedUsers { get; set; } = new List<User>();
    }
}