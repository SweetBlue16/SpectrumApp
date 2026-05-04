using Grpc.Core;
using Spectrum.API.Dtos.Votes;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Social;
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
        private readonly VoteService.VoteServiceClient _voteServiceClient;

        public VoteServiceClient(VoteService.VoteServiceClient voteServiceClient)
        {
            _voteServiceClient = voteServiceClient;
        }

        public async Task<VoteResultDto> CastReviewVoteAsync(Guid reviewId, Guid userId, bool isPositive)
        {
            try
            {
                var response = await _voteServiceClient.CastVoteAsync(new CastVoteRequest
                {
                    UserId = userId.ToString(),
                    TargetId = reviewId.ToString(),
                    TargetType = ReviewTargetType,
                    IsPositive = isPositive
                });

                return new VoteResultDto
                {
                    Success = response.Success,
                    UpdatedLikes = response.UpdatedLikes,
                    UpdatedDislikes = response.UpdatedDislikes
                };
            }
            catch (RpcException ex)
            {
                throw new SpectrumServiceUnavailableException(Constants.ErrorMessages.RpcServiceUnavailable, ex);
            }
        }
    }
}