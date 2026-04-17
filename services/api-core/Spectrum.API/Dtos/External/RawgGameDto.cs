using System.Text.Json.Serialization;

namespace Spectrum.API.Dtos.External
{
    public class RawgGameDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Released { get; set; }

        [JsonPropertyName("background_image")]
        public string BackgroundImage { get; set; }

        public double Rating { get; set; }
    }

    public class RawgPlatformWrapperDto
    {
        public RawgPlatformDto Platform { get; set; }
    }

    public class RawgPlatformDto
    {
        public string Name { get; set; }
    }
}
