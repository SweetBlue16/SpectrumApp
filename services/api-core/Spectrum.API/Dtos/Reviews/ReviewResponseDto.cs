namespace Spectrum.API.Dtos.Reviews
{
    public class ReviewResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public string UserProfileImageUrl { get; set; } = string.Empty;

        public string ProfilePicture { get; set; } = string.Empty;

        public int GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public string GameCoverUrl { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public int LikesCount { get; set; }

        public int DislikesCount { get; set; }

        public bool IsOwnReview { get; set; }
    }
}
