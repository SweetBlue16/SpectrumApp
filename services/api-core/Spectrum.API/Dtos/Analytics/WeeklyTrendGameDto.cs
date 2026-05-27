namespace Spectrum.API.Dtos.Analytics
{
    public class WeeklyTrendGameDto
    {
        public int Rank { get; set; }
        public int GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public int ReviewsCount { get; set; }
        public IReadOnlyList<WeeklyReviewDto> Reviews { get; set; } = [];
    }
}
