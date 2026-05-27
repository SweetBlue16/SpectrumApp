namespace Spectrum.API.Dtos.Analytics
{
    public class TopGameMetricDto
    {
        public int GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
