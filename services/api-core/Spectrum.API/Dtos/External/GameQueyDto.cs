using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.External
{
    public class GameQueyDto
    {
        [MaxLength(100)]
        public string? Search { get; set; }

        public string? Platforms { get; set; }

        public string? Genres { get; set; }

        [RegularExpression("^(name|released|added|created|updated|rating|-name|-released|-added|-created|-updated|-rating)$")]
        public string? Ordering { get; set; } = "-rating";

        [Range(1, 100)]
        public int PageSize { get; set; } = 20;

        [Range(1, 1000)]
        public int Page { get; set; } = 1;
    }
}
