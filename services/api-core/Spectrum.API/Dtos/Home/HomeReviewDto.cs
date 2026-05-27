namespace Spectrum.API.Dtos.Home
{
    public class HomeReviewDto
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public string GameCoverUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int Rating { get; set; }
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public int CommentsCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
