namespace Spectrum.API.Dtos.Analytics
{
    public class CryptDashboardDto
    {
        public DateTime MonthStart { get; set; }
        public DateTime MonthEnd { get; set; }
        public IReadOnlyList<NamedMetricDto> WorstGames { get; set; } = [];
        public IReadOnlyList<NamedMetricDto> GamesWithoutReviews { get; set; } = [];
    }
}
