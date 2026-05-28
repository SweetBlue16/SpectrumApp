using Grpc.Core;
using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.API.Dtos.Reports;
using Spectrum.API.Exceptions;
using Spectrum.API.Grpc.Social;
using Spectrum.API.Services.Reports;

namespace Spectrum.Tests.UnitTests.Services
{
    public class ReportServiceTests
    {
        private readonly Mock<ReportService.ReportServiceClient> _grpcClientMock;
        private readonly Mock<ILogger<ReportsService>> _loggerMock;
        private readonly ReportsService _reportsService;

        public ReportServiceTests()
        {
            _grpcClientMock = new Mock<ReportService.ReportServiceClient>();
            _loggerMock = new Mock<ILogger<ReportsService>>();
            _reportsService = new ReportsService(_grpcClientMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task TestSubmitReportAsyncWhenJavaReturnsSuccessShouldCompleteWithoutThrowing()
        {
            var reporterId = Guid.NewGuid();
            var dto = new CreateReportDto
            {
                TargetId = Guid.NewGuid(),
                TargetType = "REVIEW",
                Reason = "Inappropriate content"
            };

            var expectedResponse = new ReportResponse
            {
                Success = true,
                Message = "Report submitted successfully."
            };

            _grpcClientMock
                .Setup(c => c.SubmitReportAsync(
                    It.Is<SubmitReportRequest>(r =>
                        r.ReporterId == reporterId.ToString() &&
                        r.TargetId == dto.TargetId.ToString() &&
                        r.Reason == dto.Reason),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(expectedResponse));

            var exception = await Record.ExceptionAsync(() =>
                _reportsService.SubmitReportAsync(reporterId, dto, CancellationToken.None));

            Assert.Null(exception);
        }

        [Fact]
        public async Task TestSubmitReportAsyncWhenJavaReturnsFailureShouldThrowSpectrumBusinessException()
        {
            var reporterId = Guid.NewGuid();
            var dto = new CreateReportDto { TargetId = Guid.NewGuid(), TargetType = "REVIEW", Reason = "Spam" };

            var errorMessage = "You have already reported this content.";
            var expectedResponse = new ReportResponse
            {
                Success = false,
                Message = errorMessage
            };

            _grpcClientMock
                .Setup(c => c.SubmitReportAsync(It.IsAny<SubmitReportRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(expectedResponse));

            var exception = await Assert.ThrowsAsync<SpectrumBusinessException>(() =>
                _reportsService.SubmitReportAsync(reporterId, dto, CancellationToken.None));

            Assert.Equal(errorMessage, exception.Message);
        }

        [Fact]
        public async Task TestSubmitReportAsyncWhenGrpcFailsShouldThrowServiceUnavailableException()
        {
            var reporterId = Guid.NewGuid();
            var dto = new CreateReportDto { TargetId = Guid.NewGuid(), TargetType = "REVIEW", Reason = "Spam" };

            _grpcClientMock
                .Setup(c => c.SubmitReportAsync(It.IsAny<SubmitReportRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Throws(new RpcException(new Status(StatusCode.Unavailable, "Connection Refused")));

            var exception = await Assert.ThrowsAsync<SpectrumServiceUnavailableException>(() =>
                _reportsService.SubmitReportAsync(reporterId, dto, CancellationToken.None));

            Assert.Contains("rpcServiceUnavailable", exception.Message);
        }

        [Fact]
        public async Task TestUpdateReportStatusAsyncWhenJavaReturnsSuccessShouldCompleteWithoutThrowing()
        {
            var adminId = Guid.NewGuid();
            var reportId = "report-123";
            var dto = new UpdateReportStatusDto
            {
                NewStatus = "RESOLVED",
                ResolutionNotes = "User banned"
            };

            var expectedResponse = new ReportActionResponse
            {
                Success = true,
                Message = "Status updated"
            };

            _grpcClientMock
                .Setup(c => c.UpdateReportStatusAsync(
                    It.Is<UpdateReportStatusRequest>(r =>
                        r.ReportId == reportId &&
                        r.ModeratorId == adminId.ToString() &&
                        r.NewStatus == dto.NewStatus),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(expectedResponse));

            var exception = await Record.ExceptionAsync(() =>
                _reportsService.UpdateReportStatusAsync(reportId, adminId, dto, CancellationToken.None));

            Assert.Null(exception);
        }

        [Fact]
        public async Task TestUpdateReportStatusAsyncWhenJavaReturnsFailureShouldThrowSpectrumBusinessException()
        {
            var adminId = Guid.NewGuid();
            var expectedResponse = new ReportActionResponse { Success = false, Message = "Report not found." };

            _grpcClientMock
                .Setup(c => c.UpdateReportStatusAsync(It.IsAny<UpdateReportStatusRequest>(), null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncUnaryCall(expectedResponse));

            var exception = await Assert.ThrowsAsync<SpectrumNotFoundException>(() =>
                _reportsService.UpdateReportStatusAsync(
                    "invalid-id",
                    adminId,
                    new UpdateReportStatusDto { NewStatus = "RESOLVED" },
                    CancellationToken.None));

            Assert.Equal("Report not found.", exception.Message);
        }

        [Fact]
        public async Task TestGetReportsByStatusAsyncWhenIdsAreMongoObjectIdsShouldNotParseAsGuid()
        {
            var response = new ReportDetails
            {
                ReportId = "665f2f4fd7f2aa6f783b8123",
                ReporterId = "665f2f4fd7f2aa6f783b8124",
                TargetId = "665f2f4fd7f2aa6f783b8125",
                TargetType = "COMMENT",
                Reason = "SPAM",
                Status = "PENDING",
                ReportedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Description = "Demo report"
            };

            _grpcClientMock
                .Setup(c => c.ListReportsByStatus(
                    It.Is<ListReportsRequest>(request => request.Status == "PENDING"),
                    null, null, It.IsAny<CancellationToken>()))
                .Returns(CreateAsyncServerStreamingCall(response));

            var result = (await _reportsService.GetReportsByStatusAsync("PENDING", CancellationToken.None)).Single();

            Assert.Equal(response.ReportId, result.ReportId);
            Assert.Equal(response.ReporterId, result.ReporterId);
            Assert.Equal(response.TargetId, result.TargetId);
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

        private static AsyncServerStreamingCall<TResponse> CreateAsyncServerStreamingCall<TResponse>(params TResponse[] responses)
        {
            return new AsyncServerStreamingCall<TResponse>(
                new TestAsyncStreamReader<TResponse>(responses),
                Task.FromResult(new Metadata()),
                () => Status.DefaultSuccess,
                () => new Metadata(),
                () => { });
        }

        private sealed class TestAsyncStreamReader<T> : IAsyncStreamReader<T>
        {
            private readonly IReadOnlyList<T> _responses;
            private int _index = -1;

            public TestAsyncStreamReader(IReadOnlyList<T> responses)
            {
                _responses = responses;
            }

            public T Current { get; private set; } = default!;

            public Task<bool> MoveNext(CancellationToken cancellationToken)
            {
                _index++;
                if (_index >= _responses.Count)
                {
                    return Task.FromResult(false);
                }

                Current = _responses[_index];
                return Task.FromResult(true);
            }
        }
    }
}
