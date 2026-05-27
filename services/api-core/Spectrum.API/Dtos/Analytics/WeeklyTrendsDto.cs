namespace Spectrum.API.Dtos.Analytics
{
    public class WeeklyTrendsDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public IReadOnlyList<WeeklyTrendGameDto> Games { get; set; } = [];
    }
}
