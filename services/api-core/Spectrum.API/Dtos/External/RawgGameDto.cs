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
        public string BackgroundImage { get; set; } = string.Empty;

        /// <summary>
        /// The average user rating of the game on the external platform (e.g., 4.5).
        /// </summary>
        public double Rating { get; set; }
    }

    /// <summary>
    /// A wrapper class used to deserialize the nested platform JSON structure from the RAWG API.
    /// </summary>
    public class RawgPlatformWrapperDto
    {
        /// <summary>
        /// The underlying platform details.
        /// </summary>
        public RawgPlatformDto Platform { get; set; } = new RawgPlatformDto();
    }

    /// <summary>
    /// Represents the details of a gaming platform (e.g., PC, PlayStation 5) from the RAWG API.
    /// </summary>
    public class RawgPlatformDto
    {
        /// <summary>
        /// The name of the gaming platform.
        /// </summary>
        public string Name { get; set; } = string.Empty;
    }
}
