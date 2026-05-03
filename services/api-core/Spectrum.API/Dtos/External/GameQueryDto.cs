using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.External
{
    /// <summary>
    /// Data transfer object used to capture search, filtering, and pagination 
    /// parameters from the client when querying the external game catalog.
    /// </summary>
    public class GameQueryDto
    {
        /// <summary>
        /// A text string to search for within the game titles.
        /// </summary>
        [MaxLength(100)]
        public string? Search { get; set; }

        /// <summary>
        /// A comma-separated list of platform IDs to filter the results (e.g., "4,5" for PC and iOS).
        /// </summary>
        public string? Platforms { get; set; }

        /// <summary>
        /// A comma-separated list of genre IDs or slugs to filter the results (e.g., "action,indie").
        /// </summary>
        public string? Genres { get; set; }

        /// <summary>
        /// The field by which to sort the results. Prefix with a hyphen (-) for descending order.
        /// Supported fields: name, released, added, created, updated, rating.
        /// </summary>
        [RegularExpression("^(name|released|added|created|updated|rating|-name|-released|-added|-created|-updated|-rating)$",
            ErrorMessage = "Invalid ordering format. Use allowed fields, optionally prefixed with a hyphen (-) for descending.")]
        public string? Ordering { get; set; } = "-rating";

        /// <summary>
        /// The maximum number of game records to return in a single page.
        /// </summary>
        [Range(1, 100)]
        public int? PageSize { get; set; } 

        /// <summary>
        /// The specific page number of the results to retrieve.
        /// </summary>
        [Range(1, 1000)]
        public int Page { get; set; } = 1;
    }
}
