using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spectrum.API.Controllers;
using Spectrum.API.Dtos.Reports;
using Spectrum.API.Services.Reports;
using System.Security.Claims;

namespace Spectrum.Tests.UnitTests.Controllers
{
    public class ReportsControllerTests
    {
        private readonly Mock<IReportService> _reportsServiceMock;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _reportsServiceMock = new Mock<IReportService>();
            _controller = new ReportsController(_reportsServiceMock.Object);
        }

        [Fact]
        public async Task TestSubmitReportWhenUserIsAuthenticatedShouldReturnOk()
        {
            var userId = Guid.NewGuid();
            SetupControllerContext(userId);

            var dto = new CreateReportDto
            {
                TargetId = Guid.NewGuid(),
                TargetType = "REVIEW",
                Reason = "Inappropriate content"
            };

            _reportsServiceMock
                .Setup(s => s.SubmitReportAsync(userId, dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.SubmitReport(dto, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);

            _reportsServiceMock.Verify(s => s.SubmitReportAsync(userId, dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task TestSubmitReportWhenUserIsNotAuthenticatedShouldReturnUnauthorized()
        {
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };

            var dto = new CreateReportDto { TargetId = Guid.NewGuid(), TargetType = "REVIEW", Reason = "Spam" };

            var result = await _controller.SubmitReport(dto, CancellationToken.None);

            Assert.IsType<UnauthorizedResult>(result);

            _reportsServiceMock.Verify(s => s.SubmitReportAsync(It.IsAny<Guid>(), It.IsAny<CreateReportDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task TestUpdateReportStatusWhenValidAdminShouldReturnOk()
        {
            var adminId = Guid.NewGuid();
            SetupControllerContext(adminId);

            var reportId = "report-123";
            var dto = new UpdateReportStatusDto { NewStatus = "RESOLVED", ResolutionNotes = "Banned" };

            _reportsServiceMock
                .Setup(s => s.UpdateReportStatusAsync(reportId, adminId, dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.ResolveReport(reportId, dto, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.NotNull(okResult.Value);
        }

        private void SetupControllerContext(Guid userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
