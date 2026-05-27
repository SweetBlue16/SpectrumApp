namespace Spectrum.API.Dtos.Search
{
    public class GlobalSearchItemDto
    {
        public string Type { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Subtitle { get; set; }
        public string? ImageUrl { get; set; }
    }
}
