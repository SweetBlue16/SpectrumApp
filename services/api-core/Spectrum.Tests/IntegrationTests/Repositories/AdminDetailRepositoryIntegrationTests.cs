using Microsoft.Extensions.DependencyInjection;
using Spectrum.API.Models;
using Spectrum.API.Repositories;
using Spectrum.Tests.Helpers.Mocks;

namespace Spectrum.Tests.IntegrationTests.Repositories
{
    public class AdminDetailRepositoryIntegrationTests : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;

        public AdminDetailRepositoryIntegrationTests(CustomWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task TestAddAdminDetailAsyncWithValidUserIdShouldSaveSuccessfully()
        {
            using var scope = _factory.Services.CreateScope();
            var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
            var adminRepository = scope.ServiceProvider.GetRequiredService<IAdminDetailRepository>();

            var user = DataFakers.UserFaker.Generate();
            var createdUser = await userRepository.AddUserAsync(user);

            var adminDetail = new AdminDetail
            {
                UserId = createdUser.Id,
                FirstName = "Integration",
                LastName = "Tester",
                PhoneNumber = "+525551234567",
                Address = "Test Street 123, Dev City",
                Rfc = "TEST870123XX1"
            };

            var createdAdminDetail = await adminRepository.AddAdminDetailAsync(adminDetail);

            Assert.NotNull(createdAdminDetail);
            Assert.Equal(createdUser.Id, createdAdminDetail.UserId);
            Assert.Equal(adminDetail.Rfc, createdAdminDetail.Rfc);
        }
    }
}
