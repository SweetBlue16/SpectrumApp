using Spectrum.API.Dtos.Drops;

namespace Spectrum.API.Dtos.Home
{
    public class HomeDashboardDto
    {
        public string BannerTitle { get; set; } = "SPECTRUM";
        public string BannerSubtitle { get; set; } = "Descubre juegos, reseñas y sorteos activos.";
        public IReadOnlyList<HomeGameDto> RecentGames { get; set; } = [];
        public IReadOnlyList<HomeReviewDto> PopularReviewsToday { get; set; } = [];
        public IReadOnlyList<EventStatusDto> WeeklyDrops { get; set; } = [];
    }
}
