using Spectrum.API.Dtos.Reviews;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories;

namespace Spectrum.API.Services.Reviews
{
    public interface IReviewService
    {
        Task<ReviewResponseDto> CreateAsync(
            CreateReviewDto dto,
            Guid userId,
            CancellationToken cancellationToken = default
        );

        Task<ReviewResponseDto> GetByIdAsync(
            Guid reviewId,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<ReviewResponseDto>> GetByGameIdAsync(
            int gameId,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<ReviewResponseDto>> GetByUserIdAsync(
            Guid userId,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default
        );

        Task UpdateAsync(
            Guid reviewId,
            UpdateReviewDto dto,
            Guid userId,
            CancellationToken cancellationToken = default
        );

        Task DeleteAsync(
            Guid reviewId,
            Guid userId,
            CancellationToken cancellationToken = default
        );
    }

    public class ReviewService : IReviewService
    {
        private const string ReviewNotFoundMessage = "La resena solicitada no existe.";
        private const string ForbiddenActionMessage = "No tienes permisos para realizar esta accion.";

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

        public async Task<ReviewResponseDto> CreateAsync(
            CreateReviewDto dto,
            Guid userId,
            CancellationToken cancellationToken = default
        )
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

            var createdReview = await _reviewRepository.AddAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            return MapToResponseDto(createdReview, userId);
        }

        public async Task<ReviewResponseDto> GetByIdAsync(
            Guid reviewId,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default
        )
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            return MapToResponseDto(review, currentUserId);
        }

        public async Task<IReadOnlyList<ReviewResponseDto>> GetByGameIdAsync(
            int gameId,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default
        )
        {
            ValidateGameId(gameId);

            var reviews = await _reviewRepository.GetByGameIdAsync(gameId, cancellationToken);

            return reviews
                .Select(review => MapToResponseDto(review, currentUserId))
                .ToList();
        }

        public async Task<IReadOnlyList<ReviewResponseDto>> GetByUserIdAsync(
            Guid userId,
            Guid? currentUserId = null,
            CancellationToken cancellationToken = default
        )
        {
            var reviews = await _reviewRepository.GetByUserIdAsync(userId, cancellationToken);

            return reviews
                .Select(review => MapToResponseDto(review, currentUserId))
                .ToList();
        }

        public async Task UpdateAsync(
            Guid reviewId,
            UpdateReviewDto dto,
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var review = await GetExistingReviewAsync(reviewId, cancellationToken);
            EnsureReviewOwner(review, userId);

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

            await _reviewRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            Guid reviewId,
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var review = await GetExistingReviewAsync(reviewId, cancellationToken);
            EnsureReviewOwner(review, userId);

            review.IsDeleted = true;
            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.SaveChangesAsync(cancellationToken);
        }

        private async Task<Review> GetExistingReviewAsync(Guid reviewId, CancellationToken cancellationToken)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            return review;
        }

        private static void EnsureReviewOwner(Review review, Guid userId)
        {
            if (review.UserId != userId)
            {
                throw new SpectrumForbiddenException(ForbiddenActionMessage);
            }
        }

        private static ReviewResponseDto MapToResponseDto(Review review, Guid? currentUserId)
        {
            var profilePicture = review.User?.ProfilePicture ?? string.Empty;

            return new ReviewResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                Username = review.User?.Username ?? string.Empty,
                UserProfileImageUrl = profilePicture,
                ProfilePicture = profilePicture,
                GameId = review.GameId,
                GameTitle = string.Empty,
                GameCoverUrl = string.Empty,
                Rating = review.Rating,
                Title = string.Empty,
                Content = review.Content,
                ImageUrl = review.ImageUrl ?? string.Empty,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                LikesCount = review.LikesCount,
                DislikesCount = review.DislikesCount,
                IsOwnReview = currentUserId.HasValue && review.UserId == currentUserId.Value
            };
        }

        private static void ValidateGameId(int gameId)
        {
            if (gameId < MinimumGameId)
            {
                throw new SpectrumBusinessException("El ID del videojuego debe ser valido.");
            }
        }

        private static void ValidateRating(int rating)
        {
            if (rating is < MinimumRating or > MaximumRating)
            {
                throw new SpectrumBusinessException("La calificacion debe estar entre 1 y 5.");
            }
        }

        private static string NormalizeContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new SpectrumBusinessException("El contenido de la resena es obligatorio.");
            }

            var normalizedContent = content.Trim();

            if (normalizedContent.Length > MaximumContentLength)
            {
                throw new SpectrumBusinessException("El contenido de la resena no puede superar los 2000 caracteres.");
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
