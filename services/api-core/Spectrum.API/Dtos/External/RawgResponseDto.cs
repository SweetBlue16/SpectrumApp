using System.Text.Json.Serialization;

namespace Spectrum.API.Dtos.External
{
    /// <summary>
    /// Data transfer object representing the paginated JSON response envelope 
    /// returned by the RAWG external API when searching for games.
    /// </summary>
    public class RawgResponseDto
    {
        /// <summary>
        /// The total number of games matching the search criteria across all pages.
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// The URL to fetch the next page of results, or null if on the last page.
        /// </summary>
        public string? Next { get; set; } = string.Empty;

        /// <summary>
        /// The URL to fetch the previous page of results, or null if on the first page.
        /// </summary>
        public string? Previous { get; set; } = string.Empty;

        /// <summary>
        /// The collection of games returned for the current page.
        /// </summary>
        public List<RawgGameDto>? Results { get; set; } = [];
    }
}
