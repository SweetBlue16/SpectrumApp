using Spectrum.API.Models;

namespace Spectrum.API.Dtos.Reviews
{
    public class GameReviewDetailDto
    {
        public Game Game { get; set; } = new();

        public IReadOnlyList<ReviewResponseDto> Reviews { get; set; } = Array.Empty<ReviewResponseDto>();

        public double? AverageRating { get; set; }

        public int ReviewsCount { get; set; }
    }
}
