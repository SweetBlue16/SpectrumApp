using Microsoft.AspNetCore.Mvc.Testing;
using Spectrum.API.Dtos.Auth;
using Spectrum.Tests.Helpers.Mocks;
using System.Net;
using System.Net.Http.Json;

namespace Spectrum.Tests.IntegrationTests.Controllers
{
    public class AuthControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly HttpClient _client;
        private readonly CustomWebApplicationFactory _factory;

        public AuthControllerIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task TestRegisterWithValidDataShouldReturn201CreatedAndSaveToRealDatabase()
        {
            var registerDto = DataFakers.RegisterDtoFaker.Generate();

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);
            Assert.Equal(registerDto.Username, authResponse.Username);
            Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));
        }

        [Fact]
        public async Task TestRegisterWithDuplicateEmailShouldReturn400BadRequest()
        {
            var registerDto1 = DataFakers.RegisterDtoFaker.Generate();
            var registerDto2 = DataFakers.RegisterDtoFaker.Generate();

            registerDto2.Email = registerDto1.Email;

            await _client.PostAsJsonAsync("/api/auth/register", registerDto1);

            var response = await _client.PostAsJsonAsync("/api/auth/register", registerDto2);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Fact]
        public async Task TestRegisterAndLoginShouldReturnValidJwt()
        {
            var registerDto = DataFakers.RegisterDtoFaker.Generate();

            var registerResponse = await _client.PostAsJsonAsync("/api/auth/register", registerDto);
            registerResponse.EnsureSuccessStatusCode();

            var loginDto = new LoginDto
            {
                Email = registerDto.Email,
                Password = registerDto.Password
            };

            var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginDto);

            Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

            var authResponse = await loginResponse.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);
            Assert.False(string.IsNullOrWhiteSpace(authResponse.Token));
        }

        [Fact]
        public async Task TestRegisterAdminWithValidMasterKeyShouldSaveToBothTablesAndReturns201()
        {
            var registerAdminDto = DataFakers.RegisterAdminDtoFaker.Generate();

            var response = await _client.PostAsJsonAsync("/api/auth/register-admin", registerAdminDto);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponseDto>();
            Assert.NotNull(authResponse);
        }
    }
}
