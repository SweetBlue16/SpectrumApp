namespace Spectrum.API.Dtos.Reviews
{
    public class ReviewCommentResponseDto
    {
        public string Id { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public Guid ReviewId { get; set; }

        public string Content { get; set; } = string.Empty;

        public DateTime PublishedAt { get; set; }

        public bool IsOwnComment { get; set; }
    }
}
