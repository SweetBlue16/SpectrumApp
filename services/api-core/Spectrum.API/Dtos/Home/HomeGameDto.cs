namespace Spectrum.API.Dtos.Home
{
    public class HomeGameDto
    {
        public int GameId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
    }
}
