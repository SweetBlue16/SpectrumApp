using System.Text.Json.Serialization;

namespace Spectrum.API.Dtos.External
{
    /// <summary>
    /// Data transfer object representing the core metadata of a video game 
    /// as returned by the RAWG external API.
    /// </summary>
    public class RawgGameDto
    {
        /// <summary>
        /// The unique integer identifier assigned to the game by RAWG.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The official title of the video game.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// The release date of the game, formatted as a string (usually YYYY-MM-DD).
        /// </summary>
        public string Released { get; set; } = string.Empty;

        /// <summary>
        /// The URL pointing to the game's primary background or cover image hosted by RAWG.
        /// </summary>
        [JsonPropertyName("background_image")]
        public string? BackgroundImage { get; set; } = string.Empty;

        /// <summary>
        /// List of platform categories (PC, Xbox, PlayStation, etc.).
        /// </summary>
        [JsonPropertyName("parent_platforms")]
        public List<RawgPlatformWrapperDto>? ParentPlatforms { get; set; }

        [JsonPropertyName("ratings_count")]
        public int RatingsCount { get; set; }

        /// <summary>
        /// List of genres associated with the game.
        /// </summary>
        public List<RawgGenreDto>? Genres { get; set; }
    }

}
