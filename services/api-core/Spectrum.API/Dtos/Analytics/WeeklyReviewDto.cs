namespace Spectrum.API.Dtos.Analytics
{
    public class WeeklyReviewDto
    {
        public Guid ReviewId { get; set; }
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public int GameId { get; set; }
        public string GameTitle { get; set; } = string.Empty;
        public string GameCoverUrl { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string AttachmentUrl { get; set; } = string.Empty;
        public string AttachmentType { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
