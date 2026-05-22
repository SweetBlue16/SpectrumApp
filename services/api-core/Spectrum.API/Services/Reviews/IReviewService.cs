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
            bool isAdmin = false,
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
            bool isAdmin = false,
            CancellationToken cancellationToken = default
        );
    }

    public class ReviewService : IReviewService
    {
        private const string ReviewNotFoundMessage = "La resena solicitada no existe.";
        private const string ForbiddenActionMessage = "No tienes permisos para realizar esta accion.";

        private const int MinimumGameId = 1;
        private const int MinimumRating = 5;
        private const int MaximumRating = 10;
        private const int MaximumTitleLength = 120;
        private const int MaximumContentLength = 2000;
        private const int MaximumImageUrlLength = 255;
        private static readonly HashSet<string> AllowedMediaTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image",
            "video"
        };

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
            var title = NormalizeTitle(dto.Title);
            var content = NormalizeContent(dto.Content);
            ValidateImageUrl(dto.ImageUrl);
            ValidateMediaType(dto.ImageUrl, dto.MediaType);

            var review = new Review
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                GameId = dto.GameId,
                Rating = dto.Rating,
                Title = title,
                Content = content,
                ImageUrl = dto.ImageUrl,
                MediaType = NormalizeMediaType(dto.MediaType),
                LikesCount = 0,
                DislikesCount = 0,
                CreatedAt = DateTime.UtcNow,
                IsDeleted = false
            };

            var createdReview = await _reviewRepository.AddAsync(review, cancellationToken);
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            var persistedReview = await _reviewRepository.GetByIdAsync(createdReview.Id, cancellationToken);

            return MapToResponseDto(persistedReview ?? createdReview, userId);
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
            bool isAdmin = false,
            CancellationToken cancellationToken = default
        )
        {
            ValidateGameId(gameId);

            var reviews = await _reviewRepository.GetByGameIdAsync(gameId, cancellationToken);

            return reviews
                .Select(review => MapToResponseDto(review, currentUserId, isAdmin))
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

            if (dto.Title is not null)
            {
                review.Title = NormalizeTitle(dto.Title);
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

            if (dto.MediaType is not null)
            {
                ValidateMediaType(review.ImageUrl, dto.MediaType);
                review.MediaType = NormalizeMediaType(dto.MediaType);
            }

            review.UpdatedAt = DateTime.UtcNow;

            await _reviewRepository.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(
            Guid reviewId,
            Guid userId,
            bool isAdmin = false,
            CancellationToken cancellationToken = default
        )
        {
            var review = await GetExistingReviewAsync(reviewId, cancellationToken);
            EnsureReviewOwnerOrAdmin(review, userId, isAdmin);

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

        private static void EnsureReviewOwnerOrAdmin(Review review, Guid userId, bool isAdmin)
        {
            if (!isAdmin && review.UserId != userId)
            {
                throw new SpectrumForbiddenException(ForbiddenActionMessage);
            }
        }

        private static ReviewResponseDto MapToResponseDto(Review review, Guid? currentUserId, bool isAdmin = false)
        {
            var profilePicture = review.User?.ProfilePicture ?? string.Empty;
            var isOwnReview = currentUserId.HasValue && review.UserId == currentUserId.Value;
            var username = review.User?.Username ?? "Usuario Spectrum";

            return new ReviewResponseDto
            {
                Id = review.Id,
                UserId = review.UserId,
                Username = username,
                UserProfileImageUrl = profilePicture,
                ProfilePicture = profilePicture,
                GameId = review.GameId,
                GameTitle = string.Empty,
                GameCoverUrl = string.Empty,
                Rating = review.Rating,
                Title = review.Title,
                Content = review.Content,
                ImageUrl = review.ImageUrl ?? string.Empty,
                AttachmentUrl = review.ImageUrl ?? string.Empty,
                AttachmentType = review.MediaType ?? string.Empty,
                CreatedAt = review.CreatedAt,
                UpdatedAt = review.UpdatedAt,
                LikesCount = review.LikesCount,
                DislikesCount = review.DislikesCount,
                IsOwnReview = isOwnReview,
                CanDelete = isOwnReview || isAdmin
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
                throw new SpectrumBusinessException("La calificacion debe estar entre 5 y 10.");
            }
        }

        private static string NormalizeTitle(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                throw new SpectrumBusinessException("El titulo de la resena es obligatorio.");
            }

            var normalizedTitle = title.Trim();

            if (normalizedTitle.Length > MaximumTitleLength)
            {
                throw new SpectrumBusinessException("El titulo de la resena no puede superar los 120 caracteres.");
            }

            return normalizedTitle;
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
                throw new SpectrumBusinessException("La URL del adjunto no puede superar los 255 caracteres.");
            }
        }

        private static void ValidateMediaType(string? imageUrl, string? mediaType)
        {
            if (string.IsNullOrWhiteSpace(imageUrl) && string.IsNullOrWhiteSpace(mediaType))
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(imageUrl) || string.IsNullOrWhiteSpace(mediaType))
            {
                throw new SpectrumBusinessException("El adjunto debe incluir URL y tipo de archivo.");
            }

            if (!AllowedMediaTypes.Contains(mediaType))
            {
                throw new SpectrumBusinessException("El tipo de archivo adjunto no es valido.");
            }
        }

        private static string? NormalizeMediaType(string? mediaType)
        {
            return string.IsNullOrWhiteSpace(mediaType) ? null : mediaType.Trim().ToLowerInvariant();
        }
    }
}
