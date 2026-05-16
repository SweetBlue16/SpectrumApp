using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spectrum.API.Controllers;
using Spectrum.API.Dtos.Drops;
using Spectrum.API.Services.Drops;
using Spectrum.API.Utilities;
using System.Security.Claims;

namespace Spectrum.Tests.UnitTests.Controllers
{
    public class DropsControllerTests
    {
        private readonly Mock<IDropsService> _dropServiceMock;
        private readonly DropsController _controller;
        private readonly AdminDropsController _adminController;

        public DropsControllerTests()
        {
            _dropServiceMock = new Mock<IDropsService>();
            _controller = new DropsController(_dropServiceMock.Object);
            _adminController = new AdminDropsController(_dropServiceMock.Object);
        }

        [Fact]
        public async Task TestClaimWhenUserAuthenticatedShouldReturnOkWithWonKey()
        {
            var userId = Guid.NewGuid();
            SetupControllerUser(_controller, userId);

            var eventId = "event-123";
            var expectedKey = new WonKeyDto { EventId = eventId, AccessKeyCode = "STEAM-XYZ" };

            _dropServiceMock
                .Setup(s => s.ClaimAccessKeyAsync(userId, eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedKey);

            var result = await _controller.Claim(eventId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnedKey = Assert.IsType<WonKeyDto>(okResult.Value);
            Assert.Equal("STEAM-XYZ", returnedKey.AccessKeyCode);
        }

        [Fact]
        public async Task TestClaimWhenServiceReturnsNullShouldReturnBadRequest()
        {
            var userId = Guid.NewGuid();
            SetupControllerUser(_controller, userId);

            _dropServiceMock
                .Setup(s => s.ClaimAccessKeyAsync(userId, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((WonKeyDto?)null);

            var result = await _controller.Claim("event-123", CancellationToken.None);

            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.NotNull(badRequest.Value);
        }

        [Fact]
        public async Task TestCreateWhenCalledByAdminShouldReturnOk()
        {
            var adminId = Guid.NewGuid();
            SetupControllerUser(_adminController, adminId, Constants.Roles.Admin);

            var dto = new CreateDropEventDto
            {
                GameTitle = "Halo",
                EndDate = DateTime.UtcNow.AddDays(1),
                AccessKeys = new List<string> { "KEY-1", "KEY-2" }
            };

            _dropServiceMock
                .Setup(s => s.CreateEventAsync(dto, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _adminController.Create(dto, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            _dropServiceMock.Verify(s => s.CreateEventAsync(dto, It.IsAny<CancellationToken>()), Times.Once);
        }

        private static void SetupControllerUser(ControllerBase controller, Guid userId, string role = "REVIEWER")
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            }, "mock"));

            controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }
    }
}
