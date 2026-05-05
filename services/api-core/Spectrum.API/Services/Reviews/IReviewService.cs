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

        private const int MinimumGameId = 1;
        private const int MinimumRating = 1;
        private const int MaximumRating = 5;
        private const int MaximumContentLength = 2000;
        private const int MaximumImageUrlLength = 255;

        private readonly IReviewRepository _reviewRepository;

        public ReviewService(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        public async Task<ReviewResponseDto> CreateAsync(CreateReviewDto dto, Guid userId)
        {
            ValidateGameId(dto.GameId);
            ValidateRating(dto.Rating);
            var content = NormalizeContent(dto.Content);
            ValidateImageUrl(dto.ImageUrl);

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = dto.GameId,
                Rating = dto.Rating,
                Content = content,
                ImageUrl = dto.ImageUrl,
                LikesCount = 0,
                DislikesCount = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdReview = await _reviewRepository.AddAsync(review);
            await _reviewRepository.SaveChangesAsync();

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
            ValidateGameId(gameId);

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
                throw new SpectrumUnauthorizedException(ForbiddenActionMessage);
            }

            if (dto.Rating.HasValue)
            {
                ValidateRating(dto.Rating.Value);
                review.Rating = dto.Rating.Value;
            }

            if (dto.Content is not null)
            {
                review.Content = NormalizeContent(dto.Content);
            }

            if (dto.ImageUrl is not null)
            {
                ValidateImageUrl(dto.ImageUrl);
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
                throw new SpectrumUnauthorizedException(ForbiddenActionMessage);
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
                CreatedAt = review.CreatedAt,
                LikesCount = review.LikesCount,
                DislikesCount = review.DislikesCount
            };
        }

        private static void ValidateGameId(int gameId)
        {
            if (gameId < MinimumGameId)
            {
                throw new SpectrumBusinessException("El ID del videojuego debe ser válido.");
            }
        }

        private static void ValidateRating(int rating)
        {
            if (rating is < MinimumRating or > MaximumRating)
            {
                throw new SpectrumBusinessException("La calificación debe estar entre 1 y 5.");
            }
        }

        private static string NormalizeContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new SpectrumBusinessException("El contenido de la reseña es obligatorio.");
            }

            var normalizedContent = content.Trim();

            if (normalizedContent.Length > MaximumContentLength)
            {
                throw new SpectrumBusinessException("El contenido de la reseña no puede superar los 2000 caracteres.");
            }

            return normalizedContent;
        }

        private static void ValidateImageUrl(string? imageUrl)
        {
            if (imageUrl is not null && imageUrl.Length > MaximumImageUrlLength)
            {
                throw new SpectrumBusinessException("La URL de la imagen no puede superar los 255 caracteres.");
            }
        }
    }
}
