namespace Spectrum.API.Dtos.Analytics
{
    public class TrendsDashboardDto
    {
        public DateTime WeekStart { get; set; }
        public DateTime WeekEnd { get; set; }
        public DateTime MonthStart { get; set; }
        public DateTime MonthEnd { get; set; }
        public IReadOnlyList<NamedMetricDto> WeeklyInteractions { get; set; } = [];
        public IReadOnlyList<WeeklyReviewDto> WeeklyDiscussions { get; set; } = [];
        public IReadOnlyList<NamedMetricDto> WorstOfWeek { get; set; } = [];
        public IReadOnlyList<NamedMetricDto> BestOfWeek { get; set; } = [];
        public IReadOnlyList<NamedMetricDto> ConsoleOfMonth { get; set; } = [];
        public IReadOnlyList<NamedMetricDto> TopReviewersOfMonth { get; set; } = [];
        public IReadOnlyList<NamedMetricDto> GenresOfMonth { get; set; } = [];
    }
}
