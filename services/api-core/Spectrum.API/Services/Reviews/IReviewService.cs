using Spectrum.API.Dtos.Reviews;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories;

namespace Spectrum.API.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto, Guid userId);
        Task<ReviewResponseDto> GetByIdAsync(Guid reviewId);
        Task<IReadOnlyList<ReviewResponseDto>> GetByGameIdAsync(int gameId);
        Task<IReadOnlyList<ReviewResponseDto>> GetByUserIdAsync(Guid userId);
        Task UpdateAsync(Guid reviewId, UpdateReviewDto dto, Guid userId, bool isAdmin);
        Task DeleteAsync(Guid reviewId, Guid userId, bool isAdmin);
    }

    public class ReviewService : IReviewService
    {
        private const string ReviewNotFoundMessage = "La reseña solicitada no existe.";
        private const string ForbiddenActionMessage = "No tienes permisos para realizar esta acción.";

        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto, Guid userId)
        {
            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = dto.GameId,
                Rating = dto.Rating,
                Content = dto.Content.Trim(),
                ImageUrl = dto.ImageUrl,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdReview = await _reviewRepository.AddAsync(review);

            return MapToResponseDto(createdReview);
        }

        public async Task<ReviewResponseDto> GetByIdAsync(Guid reviewId)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            return MapToResponseDto(review);
        }

        public async Task<IReadOnlyList<ReviewResponseDto>> GetByGameIdAsync(int gameId)
        {
            var reviews = await _reviewRepository.GetByGameIdAsync(gameId);

            return reviews
                .Select(MapToResponseDto)
                .ToList();
        }

        public async Task<IReadOnlyList<ReviewResponseDto>> GetByUserIdAsync(Guid userId)
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId);

            return reviews
                .Select(MapToResponseDto)
                .ToList();
        }

        public async Task UpdateAsync(
            Guid reviewId,
            UpdateReviewDto dto,
            Guid userId,
            bool isAdmin
        )
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            if (!isAdmin && review.UserId != userId)
            {
                throw new UnauthorizedAccessException(ForbiddenActionMessage);
            }

            if (dto.Rating.HasValue)
            {
                review.Rating = dto.Rating.Value;
            }

            if (!string.IsNullOrWhiteSpace(dto.Content))
            {
                review.Content = dto.Content.Trim();
            }

            if (dto.ImageUrl is not null)
            {
                review.ImageUrl = dto.ImageUrl;
            }

            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid reviewId, Guid userId, bool isAdmin)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            if (!isAdmin && review.UserId != userId)
            {
                throw new UnauthorizedAccessException(ForbiddenActionMessage);
            }

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.SaveChangesAsync();
        }

        private static ReviewResponseDto MapToResponseDto(Review review)
        {
            return new ReviewResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                Username = review.User?.Username ?? string.Empty,
                GameId = review.GameId,
                GameTitle = string.Empty,
                Rating = review.Rating,
                Content = review.Content,
                ImageUrl = review.ImageUrl ?? string.Empty,
                CreatedAt = review.CreatedAt
            };
        }
    }
}