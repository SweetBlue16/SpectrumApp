using Microsoft.Extensions.DependencyInjection;
using Spectrum.API.Repositories;
using Spectrum.Tests.Helpers.Mocks;

namespace Spectrum.Tests.IntegrationTests.Repositories
{
    public class UserRepositoryIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public UserRepositoryIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TestAddUserAsyncShouldSaveAndGenerateId()
        {
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = DataFakers.UserFaker.Generate();
            user.Id = Guid.Empty;

            var createdUser = await repository.AddUserAsync(user);

            Assert.NotEqual(Guid.Empty, createdUser.Id);
            Assert.Equal(user.Email, createdUser.Email);
        }

        [Fact]
        public async Task TestGetUserByEmailAsyncWhenUserExistsShouldReturnUser()
        {
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = DataFakers.UserFaker.Generate();
            await repository.AddUserAsync(user);

            var retrievedUser = await repository.GetUserByEmailAsync(user.Email);

            Assert.NotNull(retrievedUser);
            Assert.Equal(user.Id, retrievedUser.Id);
            Assert.Equal(user.Username, retrievedUser.Username);
        }

        [Fact]
        public async Task TestUsernameExistsAsyncWhenUsernameIsTakenShouldReturnTrue()
        {
            using var scope = _factory.Services.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

            var user = DataFakers.UserFaker.Generate();
            await repository.AddUserAsync(user);

            var exists = await repository.UsernameExistsAsync(user.Username);

            Assert.True(exists);
        }
    }
}
