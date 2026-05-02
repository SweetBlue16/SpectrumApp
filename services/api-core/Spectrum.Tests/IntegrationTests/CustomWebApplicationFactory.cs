using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectrum.API.Data;
using Testcontainers.PostgreSql;

namespace Spectrum.Tests.IntegrationTests
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public CustomWebApplicationFactory()
        {
            Environment.SetEnvironmentVariable("JwtSettings__Secret", "EstaEsUnaClaveSecretaSuperLargaParaTesting123!");
            Environment.SetEnvironmentVariable("JwtSettings__Issuer", "SpectrumAPI");
            Environment.SetEnvironmentVariable("JwtSettings__Audience", "SpectrumClient");
            Environment.SetEnvironmentVariable("JwtSettings__ExpirationMinutes", "60");
            Environment.SetEnvironmentVariable("Admin__MasterKey", "MasterKey");
        }

        private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithDatabase("spectrum_test_db")
            .WithUsername("test_user")
            .WithPassword("test_password")
            .Build();

        public async Task InitializeAsync()
        {
            await _dbContainer.StartAsync();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                var descriptor = services.SingleOrDefault(
                    descriptor => descriptor.ServiceType == typeof(DbContextOptions<SpectrumDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<SpectrumDbContext>(options =>
                {
                    options.UseNpgsql(_dbContainer.GetConnectionString());
                });

                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var database = scope.ServiceProvider.GetRequiredService<SpectrumDbContext>();
                database.Database.Migrate();
            });
        }

        public new async Task DisposeAsync()
        {
            await _dbContainer.StopAsync();
            await _dbContainer.DisposeAsync();
        }
    }
}
