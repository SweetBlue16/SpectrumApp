using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;
using Spectrum.API.Dtos.External;
using Spectrum.API.Exceptions;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities;
using System.Net;
using System.Text.Json;

namespace Spectrum.Tests.UnitTests.Services
{
    public class GameServiceTests
    {
        private readonly Mock<IConfiguration> _configMock;
        private readonly Mock<ILogger<GameService>> _loggerMock;

        public GameServiceTests()
        {
            _configMock = new Mock<IConfiguration>();
            _configMock.Setup(c => c["RawgApi:ApiKey"]).Returns("FakeApiKey123");
            _loggerMock = new Mock<ILogger<GameService>>();
        }

        [Fact]
        public async Task TestGetGameDetailsAsyncWhenGameExistsShouldReturnRawgGameDto()
        {
            int gameId = 3498;
            var expectedGame = new RawgGameDto { Id = gameId, Name = "Grand Theft Auto V", Rating = 4.48 };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, expectedGame);
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var result = await gameService.GetGameDetailsAsync(gameId);

            Assert.NotNull(result);
            Assert.Equal(expectedGame.Id, result.Id);
            Assert.Equal(expectedGame.Name, result.Name);
        }

        [Fact]
        public async Task TestGetGameDetailsAsyncWhenGameNotFoundShouldThrowSpectrumNotFoundException()
        {
            int gameId = 999999;
            var httpClient = CreateMockHttpClient(HttpStatusCode.NotFound, new { detail = "Not found." });
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<SpectrumNotFoundException>(() =>
                gameService.GetGameDetailsAsync(gameId));

            Assert.Equal(Constants.ErrorMessages.ResourceNotFound, exception.Message);
        }

        [Fact]
        public async Task TestGetGameDetailsAsyncWhenNullResponseShouldThrowSpectrumNotFoundException()
        {
            int gameId = 123;
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, null);
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<SpectrumNotFoundException>(() =>
                gameService.GetGameDetailsAsync(gameId));

            Assert.Equal(Constants.ErrorMessages.ResourceNotFound, exception.Message);
        }

        [Fact]
        public async Task TestGetGameDetailsAsyncWhenHttpRequestExceptionNot404ShouldThrowSpectrumBusinessException()
        {
            int gameId = 123;
            var httpClient = CreateMockHttpClient(HttpStatusCode.InternalServerError, new { error = "Server Crash" });
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<SpectrumBusinessException>(() =>
                gameService.GetGameDetailsAsync(gameId));

            Assert.Equal(Constants.ErrorMessages.ExternalCatalogUnavailable, exception.Message);
        }

        [Fact]
        public async Task SearchGamesAsyncWhenServiceUnavailableShouldThrowSpectrumBusinessException()
        {
            var queryDto = new GameQueryDto { Search = "Halo" };

            var httpClient = CreateMockHttpClient(HttpStatusCode.ServiceUnavailable, new { });
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var exception = await Assert.ThrowsAsync<SpectrumBusinessException>(() =>
                gameService.SearchGamesAsync(queryDto));

            Assert.Equal(Constants.ErrorMessages.ExternalCatalogUnavailable, exception.Message);
        }

        [Fact]
        public async Task TestSearchGamesAsyncWhenValidQueryShouldReturnCollectionOfGames()
        {
            var queryDto = new GameQueryDto { Search = "Halo" };
            var expectedResponse = new RawgResponseDto
            {
                Count = 1,
                Results = new List<RawgGameDto> { new RawgGameDto { Id = 1, Name = "Halo: Combat Evolved" } }
            };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, expectedResponse);
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var result = await gameService.SearchGamesAsync(queryDto);

            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Halo: Combat Evolved", result.First().Name);
        }

        [Fact]
        public async Task TestSearchGamesAsyncWhenNullResponseShouldReturnEmptyCollection()
        {
            var queryDto = new GameQueryDto { Search = "NonExistentGame" };

            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, null);
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var result = await gameService.SearchGamesAsync(queryDto);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task TestSearchGamesAsyncWhenResultsAreNullShouldReturnEmptyCollection()
        {
            var queryDto = new GameQueryDto { Search = "AnotherNonExistentGame" };

            var rawgResponse = new RawgResponseDto { Count = 0, Results = null };
            var httpClient = CreateMockHttpClient(HttpStatusCode.OK, rawgResponse);
            var gameService = new GameService(httpClient, _configMock.Object, _loggerMock.Object);

            var result = await gameService.SearchGamesAsync(queryDto);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        private HttpClient CreateMockHttpClient(HttpStatusCode statusCode, object responseContent)
        {
            var handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = statusCode,
                    Content = new StringContent(JsonSerializer.Serialize(responseContent))
                })
                .Verifiable();

            return new HttpClient(handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.rawg.io/api/")
            };
        }
    }
}
