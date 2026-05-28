namespace Spectrum.API.Dtos.Media
{
    /// <summary>
    /// Represents a summary of a game clip optimized for profile views.
    /// Maps perfectly with the frontend ClipData constraints.
    /// </summary>
    public class GameClipSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? GameName { get; set; }
        public string Url { get; set; } = string.Empty;
        public int LikesCount { get; set; }
        public int DislikesCount { get; set; }
        public string? UserVote { get; set; }
        public Guid UserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
