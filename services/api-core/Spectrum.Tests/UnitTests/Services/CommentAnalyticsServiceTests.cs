using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Services.Analytics;

namespace Spectrum.Tests.UnitTests.Services
{
    public class CommentAnalyticsServiceTests
    {
        private readonly Mock<CommentService.CommentServiceClient> _clientMock = new();
        private readonly Mock<ILogger<CommentAnalyticsService>> _loggerMock = new();

        [Fact]
        public async Task GetCommentCountsAsyncWhenServiceReturnsCountsShouldMapMissingReviewsToZero()
        {
            var firstReviewId = Guid.NewGuid();
            var secondReviewId = Guid.NewGuid();
            _clientMock
                .Setup(client => client.GetCommentCountsAsync(
                    It.Is<GetCommentCountsRequest>(request => request.ReviewIds.Count == 2 && request.From > 0 && request.To > 0),
                    null,
                    null,
                    It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(new CommentCountsResponse
                {
                    Counts = { new CommentCount { ReviewId = firstReviewId.ToString(), Count = 7 } }
                }));
            var service = new CommentAnalyticsService(_clientMock.Object, _loggerMock.Object);

            var result = await service.GetCommentCountsAsync(
                new[] { firstReviewId, secondReviewId },
                DateTime.UtcNow.AddDays(-1),
                DateTime.UtcNow,
                CancellationToken.None
            );

            Assert.Equal(7, result[firstReviewId]);
            Assert.Equal(0, result[secondReviewId]);
            _clientMock.Verify(client => client.GetCommentCountsAsync(
                It.IsAny<GetCommentCountsRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetCommentCountsAsyncWhenReviewListIsEmptyShouldNotCallGrpc()
        {
            var service = new CommentAnalyticsService(_clientMock.Object, _loggerMock.Object);

            var result = await service.GetCommentCountsAsync([], cancellationToken: CancellationToken.None);

            Assert.Empty(result);
            _clientMock.Verify(client => client.GetCommentCountsAsync(
                It.IsAny<GetCommentCountsRequest>(),
                null,
                null,
                It.IsAny<CancellationToken>()), Times.Never);
        }

        private static AsyncUnaryCall<TResponse> CreateAsyncUnaryCall<TResponse>(TResponse response)
        {
            return new AsyncUnaryCall<TResponse>(
                Task.FromResult(response),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }
    }
}
