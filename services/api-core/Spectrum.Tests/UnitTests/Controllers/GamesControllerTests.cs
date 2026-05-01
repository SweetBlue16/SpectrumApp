using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Spectrum.API.Controllers;
using Spectrum.API.Dtos.External;
using Spectrum.API.Services.External;

namespace Spectrum.Tests.UnitTests.Controllers
{
    public class GamesControllerTests
    {
        private readonly Mock<IGameService> _gameServiceMock;
        private readonly GamesController _gamesController;

        public GamesControllerTests()
        {
            _gameServiceMock = new Mock<IGameService>();
            _gamesController = new GamesController(_gameServiceMock.Object);
        }

        [Fact]
        public async Task TestSearchWhenValidQueryShouldReturnOkWithGamesCollection()
        {
            var queryDto = new GameQueryDto { Search = "Halo", PageSize = 10 };
            var expectedGames = new List<RawgGameDto>
            {
                new RawgGameDto { Id = 1, Name = "Halo: Combat Evolved" },
                new RawgGameDto { Id = 2, Name = "Halo 2" }
            };

            _gameServiceMock.Setup(s => s.SearchGamesAsync(queryDto)).ReturnsAsync(expectedGames);

            var result = await _gamesController.Search(queryDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedGames = Assert.IsAssignableFrom<IEnumerable<RawgGameDto>>(okResult.Value);
            Assert.Equal(2, returnedGames.Count());

            _gameServiceMock.Verify(s => s.SearchGamesAsync(queryDto), Times.Once);
        }

        [Fact]
        public async Task TestSearchWhenNoResultsShouldReturnOkWithEmptyCollection()
        {
            var queryDto = new GameQueryDto { Search = "NonExistentGame" };
            var expectedGames = Enumerable.Empty<RawgGameDto>();

            _gameServiceMock.Setup(s => s.SearchGamesAsync(queryDto)).ReturnsAsync(expectedGames);

            var result = await _gamesController.Search(queryDto);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedGames = Assert.IsAssignableFrom<IEnumerable<RawgGameDto>>(okResult.Value);
            Assert.Empty(returnedGames);

            _gameServiceMock.Verify(s => s.SearchGamesAsync(queryDto), Times.Once);
        }

        [Fact]
        public async Task TestGetDetailsWhenGameExistsShouldReturnOkWithGameDetails()
        {
            int gameId = 3498;
            var expectedGame = new RawgGameDto { Id = gameId, Name = "Grand Theft Auto V", Rating = 4.48 };

            _gameServiceMock.Setup(s => s.GetGameDetailsAsync(gameId)).ReturnsAsync(expectedGame);

            var result = await _gamesController.GetDetails(gameId);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var returnedGame = Assert.IsType<RawgGameDto>(okResult.Value);
            Assert.Equal(expectedGame.Id, returnedGame.Id);
            Assert.Equal(expectedGame.Name, returnedGame.Name);

            _gameServiceMock.Verify(s => s.GetGameDetailsAsync(gameId), Times.Once);
        }
    }
}
