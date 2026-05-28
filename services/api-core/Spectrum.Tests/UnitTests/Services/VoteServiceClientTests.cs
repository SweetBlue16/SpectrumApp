using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Repositories;
using Spectrum.API.Services.Votes;

namespace Spectrum.Tests.UnitTests.Services
{
    public class VoteServiceClientTests
    {
        [Fact]
        public async Task CastReviewVoteAsyncWhenReviewDoesNotExistShouldThrowNotFound()
        {
            var reviewRepositoryMock = new Mock<IReviewRepository>();
            var loggerMock = new Mock<ILogger<VoteServiceClient>>();
            var grpcClient = new VoteService.VoteServiceClient(new Mock<CallInvoker>().Object);
            var voteService = new VoteServiceClient(
                grpcClient,
                reviewRepositoryMock.Object,
                loggerMock.Object
            );
            var reviewId = Guid.NewGuid();

            reviewRepositoryMock
                .Setup(repository => repository.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Spectrum.API.Models.Review?)null);

            await Assert.ThrowsAsync<SpectrumNotFoundException>(() =>
                voteService.CastReviewVoteAsync(reviewId, Guid.NewGuid(), true));

            reviewRepositoryMock.Verify(
                repository => repository.UpdateCountersAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }

        [Fact]
        public async Task CastReviewVoteAsyncWhenReviewBelongsToUserShouldThrowForbidden()
        {
            var ownerId = Guid.NewGuid();
            var reviewId = Guid.NewGuid();
            var reviewRepositoryMock = new Mock<IReviewRepository>();
            var loggerMock = new Mock<ILogger<VoteServiceClient>>();
            var grpcClient = new VoteService.VoteServiceClient(new Mock<CallInvoker>().Object);
            var voteService = new VoteServiceClient(
                grpcClient,
                reviewRepositoryMock.Object,
                loggerMock.Object
            );

            reviewRepositoryMock
                .Setup(repository => repository.GetByIdAsync(reviewId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Spectrum.API.Models.Review { Id = reviewId, UserId = ownerId });

            await Assert.ThrowsAsync<SpectrumForbiddenException>(() =>
                voteService.CastReviewVoteAsync(reviewId, ownerId, true));

            reviewRepositoryMock.Verify(
                repository => repository.UpdateCountersAsync(
                    It.IsAny<Guid>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<CancellationToken>()
                ),
                Times.Never
            );
        }
    }
}
