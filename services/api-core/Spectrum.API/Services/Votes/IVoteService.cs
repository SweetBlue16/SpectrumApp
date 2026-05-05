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
        Task<VoteResultDto> CastReviewVoteAsync(Guid reviewId, Guid userId, bool isPositive);
    }

    public class VoteServiceClient : IVoteService
    {
        private const string ReviewTargetType = "REVIEW";
        private const string ReviewNotFoundMessage = "La reseña solicitada no existe.";

        private readonly VoteService.VoteServiceClient _voteServiceClient;
        private readonly IReviewRepository _reviewRepository;

        public VoteServiceClient(
            VoteService.VoteServiceClient voteServiceClient,
            IReviewRepository reviewRepository
        )
        {
            _voteServiceClient = voteServiceClient;
            _reviewRepository = reviewRepository;
        }

        public async Task<VoteResultDto> CastReviewVoteAsync(Guid reviewId, Guid userId, bool isPositive)
        {
            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review is null)
            {
                throw new SpectrumNotFoundException(ReviewNotFoundMessage);
            }

            try
            {
                var response = await _voteServiceClient.CastVoteAsync(new CastVoteRequest
                {
                    UserId = userId.ToString(),
                    TargetId = reviewId.ToString(),
                    TargetType = ReviewTargetType,
                    IsPositive = isPositive
                });

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
                        result.UpdatedDislikes
                    );
                    await _reviewRepository.SaveChangesAsync();
                }

                return result;
            }
            catch (RpcException ex)
            {
                if (ex.StatusCode == StatusCode.InvalidArgument)
                {
                    throw new SpectrumBusinessException(ex.Status.Detail, ex);
                }

                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable, ex);
            }
        }
    }
}
