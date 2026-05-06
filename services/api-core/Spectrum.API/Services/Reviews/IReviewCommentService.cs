using Grpc.Core;
using Spectrum.API.Dtos.Reviews;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Models;
using Spectrum.API.Repositories;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Reviews
{
    public interface IReviewCommentService
    {
        Task<ReviewCommentResponseDto> CreateAsync(
            Guid reviewId,
            CreateReviewCommentDto dto,
            Guid userId,
            CancellationToken cancellationToken = default
        );

        Task<IReadOnlyList<ReviewCommentResponseDto>> GetByReviewAsync(
            Guid reviewId,
            Guid? currentUserId = null,
            int page = 1,
            CancellationToken cancellationToken = default
        );
    }

    public class ReviewCommentService : IReviewCommentService
    {
        private const int MinimumPage = 1;
        private const int MaximumContentLength = 1000;
        private const string ReviewNotFoundMessage = "La resena solicitada no existe.";
        private const string SelfCommentForbiddenMessage = "No puedes responder tu propia resena.";

        private readonly CommentService.CommentServiceClient _commentServiceClient;
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<ReviewCommentService> _logger;

        public ReviewCommentService(
            CommentService.CommentServiceClient commentServiceClient,
            IReviewRepository reviewRepository,
            ILogger<ReviewCommentService> logger
        )
        {
            _commentServiceClient = commentServiceClient;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<ReviewCommentResponseDto> CreateAsync(
            Guid reviewId,
            CreateReviewCommentDto dto,
            Guid userId,
            CancellationToken cancellationToken = default
        )
        {
            var review = await GetExistingReviewAsync(reviewId, cancellationToken);

            if (review.UserId == userId)
            {
                throw new SpectrumForbiddenException(SelfCommentForbiddenMessage);
            }

            var content = NormalizeContent(dto.Content);

            try
            {
                var response = await _commentServiceClient.PublishCommentAsync(
                    new PublishCommentRequest
                    {
                        UserId = userId.ToString(),
                        ReviewId = reviewId.ToString(),
                        Content = content
                    },
                    cancellationToken: cancellationToken
                );

                return MapCommentResponse(response, userId, reviewId, content);
            }
            catch (RpcException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Review comment publish failed for reviewId={ReviewId} userId={UserId} status={StatusCode}",
                    reviewId,
                    userId,
                    ex.StatusCode
                );
                throw MapRpcException(ex);
            }
        }

        public async Task<IReadOnlyList<ReviewCommentResponseDto>> GetByReviewAsync(
            Guid reviewId,
            Guid? currentUserId = null,
            int page = 1,
            CancellationToken cancellationToken = default
        )
        {
            await GetExistingReviewAsync(reviewId, cancellationToken);
            var normalizedPage = page < MinimumPage ? MinimumPage : page;

            try
            {
                using var call = _commentServiceClient.GetCommentsByReview(
                    new GetCommentsRequest
                    {
                        ReviewId = reviewId.ToString(),
                        Page = normalizedPage
                    },
                    cancellationToken: cancellationToken
                );

                var comments = new List<ReviewCommentResponseDto>();

                while (await call.ResponseStream.MoveNext(cancellationToken))
                {
                    comments.Add(MapCommentResponse(call.ResponseStream.Current, currentUserId));
                }

                return comments;
            }
            catch (RpcException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Review comments fetch failed for reviewId={ReviewId} status={StatusCode}",
                    reviewId,
                    ex.StatusCode
                );
                throw MapRpcException(ex);
            }
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

        private static ReviewCommentResponseDto MapCommentResponse(
            CommentResponse response,
            Guid? currentUserId
        )
        {
            var userId = TryParseGuid(response.UserId);
            var reviewId = TryParseGuid(response.ReviewId);

            return new ReviewCommentResponseDto
            {
                Id = response.CommentId,
                UserId = userId,
                ReviewId = reviewId,
                Content = response.Content,
                PublishedAt = ToDateTime(response.PublishedAt),
                IsOwnComment = currentUserId.HasValue && userId == currentUserId.Value
            };
        }

        private static ReviewCommentResponseDto MapCommentResponse(
            CommentResponse response,
            Guid fallbackUserId,
            Guid fallbackReviewId,
            string fallbackContent
        )
        {
            return new ReviewCommentResponseDto
            {
                Id = response.CommentId,
                UserId = TryParseGuid(response.UserId, fallbackUserId),
                ReviewId = TryParseGuid(response.ReviewId, fallbackReviewId),
                Content = string.IsNullOrWhiteSpace(response.Content) ? fallbackContent : response.Content,
                PublishedAt = response.PublishedAt > 0 ? ToDateTime(response.PublishedAt) : DateTime.UtcNow,
                IsOwnComment = true
            };
        }

        private static Guid TryParseGuid(string value, Guid fallback = default)
        {
            return Guid.TryParse(value, out var parsedValue) ? parsedValue : fallback;
        }

        private static DateTime ToDateTime(long unixTimeMilliseconds)
        {
            return unixTimeMilliseconds > 0
                ? DateTimeOffset.FromUnixTimeMilliseconds(unixTimeMilliseconds).UtcDateTime
                : DateTime.MinValue;
        }

        private static string NormalizeContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new SpectrumBusinessException("El comentario es obligatorio.");
            }

            var normalizedContent = content.Trim();

            if (normalizedContent.Length > MaximumContentLength)
            {
                throw new SpectrumBusinessException("El comentario no puede superar los 1000 caracteres.");
            }

            return normalizedContent;
        }

        private static Exception MapRpcException(RpcException exception)
        {
            return exception.StatusCode switch
            {
                StatusCode.InvalidArgument => new SpectrumBusinessException(exception.Status.Detail, exception),
                StatusCode.NotFound => new SpectrumNotFoundException(exception.Status.Detail),
                StatusCode.PermissionDenied => new SpectrumForbiddenException(exception.Status.Detail),
                _ => new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable, exception)
            };
        }
    }
}
