using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Spectrum.API.Dtos.External;
using Spectrum.API.Models;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Spectrum.Tests.IntegrationTests.Controllers
{
    public class GamesControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly WebApplicationFactory<Program> _factory;
        private readonly Mock<IGameService> _gameServiceMock;

        public GamesControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _gameServiceMock = new Mock<IGameService>();
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureTestServices(services =>
                {
                    services.AddScoped<IGameService>(_ => _gameServiceMock.Object);
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task TestSearchWithoutAuthTokenShouldReturn401Unauthorized()
        {
            var response = await _client.GetAsync("/api/games/search?search=Halo");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task TestSearchWithValidAuthShouldReturn200OkAndData()
        {
            AuthenticateClient(Constants.Roles.Reviewer);
            var expectedGames = new List<RawgGameDto>
            {
                new RawgGameDto { Id = 1, Name = "Mocked Game" }
            };
            var expectedResult = new PagedResult<RawgGameDto>
            {
                Items = expectedGames,
                TotalCount = expectedGames.Count,
                Page = 1,
                PageSize = 10
            };

            _gameServiceMock
                .Setup(s => s.SearchGamesAsync(It.IsAny<GameQueryDto>()))
                .ReturnsAsync(expectedResult);

            var response = await _client.GetAsync("/api/games/search?search=Mocked&pageSize=10");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var data = await response.Content.ReadFromJsonAsync<PagedResult<RawgGameDto>>();
            Assert.NotNull(data);
            Assert.Single(data.Items);
            Assert.Equal("Mocked Game", data.Items.First().Name);
        }

        [Fact]
        public async Task TestGetDetailsWithValidAuthShouldReturn200Ok()
        {
            AuthenticateClient();
            int gameId = 999;
            var expectedGame = new RawgGameDto { Id = gameId, Name = "Mocked Detail Game" };

            _gameServiceMock
                .Setup(s => s.GetGameDetailsAsync(gameId))
                .ReturnsAsync(expectedGame);

            var response = await _client.GetAsync($"/api/games/{gameId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var data = await response.Content.ReadFromJsonAsync<RawgGameDto>();
            Assert.NotNull(data);
            Assert.Equal(gameId, data.Id);
        }

        private void AuthenticateClient(string role = Constants.Roles.Reviewer)
        {
            var config = _factory.Services.GetRequiredService<IConfiguration>();
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "integration_tester",
                Email = "tester@spectrum.com",
                Role = role
            };
            var token = AuthUtilities.GenerateJwtToken(user, config);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}
