using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Drops;
using Spectrum.API.Services.Drops;
using Spectrum.API.Utilities;

namespace Spectrum.Tests.UnitTests.Services
{
    public class DropsServiceTests
    {
        private readonly Mock<DropService.DropServiceClient> _grpcClientMock;
        private readonly Mock<ILogger<DropsService>> _loggerMock;
        private readonly DropsService _dropService;

        public DropsServiceTests()
        {
            _grpcClientMock = new Mock<DropService.DropServiceClient>();
            _loggerMock = new Mock<ILogger<DropsService>>();
            _dropService = new DropsService(_grpcClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task TestClaimAccessKeyAsyncWhenJavaServiceReturnsSuccessShouldReturnWonKeyDto()
        {
            var userId = Guid.NewGuid();
            var eventId = "event-123";
            var expectedKeyCode = "STEAM-XYZ-123";
            var claimedAtEpoch = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var expectedGrpcResponse = new ClaimKeyResponse
            {
                Success = true,
                AccessKeyCode = expectedKeyCode,
                ClaimedAt = claimedAtEpoch
            };

            _grpcClientMock
                .Setup(c => c.ClaimAccessKeyAsync(
                    It.Is<ClaimKeyRequest>(r => r.UserId == userId.ToString() && r.EventId == eventId),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(expectedGrpcResponse));

            var result = await _dropService.ClaimAccessKeyAsync(userId, eventId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(eventId, result.EventId);
            Assert.Equal(expectedKeyCode, result.AccessKeyCode);
            Assert.Equal(DateTimeOffset.FromUnixTimeMilliseconds(claimedAtEpoch).UtcDateTime, result.ClaimedAt);
        }

        [Fact]
        public async Task TestClaimAccessKeyAsyncWhenJavaServiceReturnsFailureShouldReturnNull()
        {
            var userId = Guid.NewGuid();
            var eventId = "event-123";

            var expectedGrpcResponse = new ClaimKeyResponse
            {
                Success = false,
                AccessKeyCode = string.Empty
            };

            _grpcClientMock
                .Setup(c => c.ClaimAccessKeyAsync(It.IsAny<ClaimKeyRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(expectedGrpcResponse));

            var result = await _dropService.ClaimAccessKeyAsync(userId, eventId, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task TestClaimAccessKeyAsyncWhenJavaServiceIsDownShouldThrowSpectrumServiceUnavailableException()
        {
            var userId = Guid.NewGuid();
            var eventId = "event-123";

            _grpcClientMock
                .Setup(c => c.ClaimAccessKeyAsync(It.IsAny<ClaimKeyRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Unavailable, "Service Unavailable")));

            var exception = await Assert.ThrowsAsync<SpectrumServiceUnavailableException>(() =>
                _dropService.ClaimAccessKeyAsync(userId, eventId, CancellationToken.None));

            Assert.Equal(Constants.ErrorMessages.RpcServiceUnavailable, exception.Message);
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
