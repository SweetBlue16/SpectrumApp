namespace Spectrum.API.Dtos.Drops
{
    public class CreateDropEventDto
    {
        public string GameTitle { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public required DateTime EndDate { get; set; }
        public List<string> AccessKeys { get; set; } = new();
    }
}
