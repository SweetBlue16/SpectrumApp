namespace Spectrum.API.Dtos.Analytics
{
    public class GlobalMetricsDto
    {
        public DateTime WindowStart { get; set; }
        public DateTime WindowEnd { get; set; }
        public IReadOnlyList<MetricPointDto> NewUsers { get; set; } = [];
        public IReadOnlyList<MetricPointDto> NewReviews { get; set; } = [];
        public IReadOnlyList<TopGameMetricDto> MostSearchedGames { get; set; } = [];
    }
}
