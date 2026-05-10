using Grpc.Core;
using Spectrum.API.Dtos.Votes;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Repositories;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Votes
{
    public interface IVoteService
    {
        Task<VoteResultDto> CastReviewVoteAsync(
            Guid reviewId,
            Guid userId,
            bool isPositive,
            CancellationToken cancellationToken = default
        );
    }

    public class VoteServiceClient : IVoteService
    {
        private const string ReviewTargetType = "REVIEW";
        private const string ReviewNotFoundMessage = "La resena solicitada no existe.";

        private readonly VoteService.VoteServiceClient _voteServiceClient;
        private readonly IReviewRepository _reviewRepository;
        private readonly ILogger<VoteServiceClient> _logger;

        public VoteServiceClient(
            VoteService.VoteServiceClient voteServiceClient,
            IReviewRepository reviewRepository,
            ILogger<VoteServiceClient> logger
        )
        {
            _voteServiceClient = voteServiceClient;
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<VoteResultDto> CastReviewVoteAsync(
            Guid reviewId,
            Guid userId,
            bool isPositive,
            CancellationToken cancellationToken = default
        )
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId, cancellationToken);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            try
            {
                var response = await _voteServiceClient.CastVoteAsync(
                    new CastVoteRequest
                    {
                        UserId = userId.ToString(),
                        TargetId = reviewId.ToString(),
                        TargetType = ReviewTargetType,
                        IsPositive = isPositive
                    },
                    cancellationToken: cancellationToken
                );

                var result = new VoteResultDto
                {
                    Success = response.Success,
                    UpdatedLikes = response.UpdatedLikes,
                    UpdatedDislikes = response.UpdatedDislikes
                };

                if (result.Success)
                {
                    await _reviewRepository.UpdateCountersAsync(
                        reviewId,
                        result.UpdatedLikes,
                        result.UpdatedDislikes,
                        cancellationToken
                    );
                    await _reviewRepository.SaveChangesAsync(cancellationToken);
                }

                return result;
            }
            catch (RpcException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Review vote failed for reviewId={ReviewId} userId={UserId} status={StatusCode}",
                    reviewId,
                    userId,
                    ex.StatusCode
                );

                if (ex.StatusCode == StatusCode.InvalidArgument)
                {
                    throw new SpectrumBusinessException(ex.Status.Detail, ex);
                }

                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable, ex);
            }
        }
    }
}
