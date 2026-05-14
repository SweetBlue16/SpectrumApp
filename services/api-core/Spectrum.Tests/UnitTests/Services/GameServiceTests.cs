using Microsoft.Extensions.Logging;
using Moq;
using Spectrum.API.Dtos.External;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities;

namespace Spectrum.Tests.UnitTests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IGameRepository> _gameRepoMock;
        private readonly Mock<IReviewRepository> _reviewRepoMock;
        private readonly Mock<ILogger<GameService>> _loggerMock;
        private readonly GameService _gameService;

        public GameServiceTests()
        {
            _gameRepoMock = new Mock<IGameRepository>();
            _reviewRepoMock = new Mock<IReviewRepository>();
            _loggerMock = new Mock<ILogger<GameService>>();

            _gameService = new GameService(_gameRepoMock.Object, _reviewRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetGameDetailsAsync_WhenGameExists_ShouldReturnGame()
        {
            int gameId = 3498;
            var expectedGame = new Game { Id = Guid.NewGuid(), RawgId = gameId, Title = "Grand Theft Auto V" };

            _gameRepoMock.Setup(repo => repo.GetById(gameId)).Returns(expectedGame);
            _reviewRepoMock.Setup(repo => repo.GetByGameIdAsync(gameId)).ReturnsAsync(new List<Review>());

            var result = await _gameService.GetGameDetailsAsync(gameId);

            Assert.NotNull(result);
            Assert.Equal(expectedGame.RawgId, result.RawgId);
            Assert.Equal(expectedGame.Title, result.Title);
        }

        [Fact]
        public async Task GetGameDetailsAsync_WhenGameNotFound_ShouldThrowSpectrumNotFoundException()
        {
            int gameId = 999999;
            _gameRepoMock.Setup(repo => repo.GetById(gameId)).Returns((Game?)null);

            var exception = await Assert.ThrowsAsync<SpectrumNotFoundException>(() =>
                _gameService.GetGameDetailsAsync(gameId));

            Assert.Equal(Constants.ErrorMessages.ResourceNotFound, exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsync_WhenValidQuery_ShouldReturnPagedResultWithRatings()
        {
            var queryDto = new GameQueryDto { Search = "Halo" };
            var mockGames = new List<Game> { new Game { Id = Guid.NewGuid(), RawgId = 1, Title = "Halo: Combat Evolved" } };

            _gameRepoMock.Setup(repo => repo.Search(queryDto)).Returns((mockGames, 1));

            var mockRatings = new Dictionary<int, double> { { 1, 4.5 } };
            _reviewRepoMock.Setup(repo => repo.GetAverageRatingsAsync()).ReturnsAsync(mockRatings);

            var result = await _gameService.SearchGamesAsync(queryDto);

            Assert.NotNull(result);
            Assert.Single(result.Items);
            Assert.Equal("Halo: Combat Evolved", result.Items.First().Title);
            Assert.Equal(4.5, result.Items.First().InternalRating);
        }

        [Fact]
        public async Task SearchGamesAsync_WhenNoResults_ShouldReturnEmptyCollection()
        {
            var queryDto = new GameQueryDto { Search = "NonExistentGame" };
            var mockGames = new List<Game>();

            _gameRepoMock.Setup(repo => repo.Search(queryDto)).Returns((mockGames, 0));
            _reviewRepoMock.Setup(repo => repo.GetAverageRatingsAsync()).ReturnsAsync(new Dictionary<int, double>());

            var result = await _gameService.SearchGamesAsync(queryDto);

            Assert.NotNull(result);
            Assert.Empty(result.Items);
        }
    }
}
