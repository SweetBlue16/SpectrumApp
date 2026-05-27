using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spectrum.API.Controllers;
using Spectrum.API.Services.Seeding;

namespace Spectrum.Tests.UnitTests.Controllers
{
    public class DevelopmentSeedControllerTests
    {
        [Fact]
        public async Task SeedDemoWhenEnvironmentIsProductionShouldReturnNotFoundAndNotRunSeed()
        {
            var seedMock = new Mock<IDemoSeedService>();
            var environmentMock = new Mock<IWebHostEnvironment>();
            environmentMock.SetupGet(environment => environment.EnvironmentName).Returns("Production");
            var controller = new DevelopmentSeedController(seedMock.Object, environmentMock.Object);

            var result = await controller.SeedDemo(CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
            seedMock.Verify(service => service.SeedAsync(It.IsAny<CancellationToken>()), Times.Never);
        }

        [Fact]
        public async Task CleanupDemoWhenEnvironmentIsProductionShouldReturnNotFoundAndNotRunCleanup()
        {
            var seedMock = new Mock<IDemoSeedService>();
            var environmentMock = new Mock<IWebHostEnvironment>();
            environmentMock.SetupGet(environment => environment.EnvironmentName).Returns("Production");
            var controller = new DevelopmentSeedController(seedMock.Object, environmentMock.Object);

            var result = await controller.CleanupDemo(CancellationToken.None);

            result.Should().BeOfType<NotFoundResult>();
            seedMock.Verify(service => service.CleanupAsync(It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}
