namespace Spectrum.API.Dtos.Reviews
{
    public class ReviewResponseDto
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }

        public string Username { get; set; } = string.Empty;

        public int GameId { get; set; }

        public string GameTitle { get; set; } = string.Empty;

        public int Rating { get; set; }

        public string Content { get; set; } = string.Empty;

        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
    }
}