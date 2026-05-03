using Bogus;
using Spectrum.API.Dtos.Auth;
using Spectrum.API.Models;
using Spectrum.API.Utilities;

namespace Spectrum.Tests.Helpers.Mocks
{
    public static class DataFakers
    {
        public static Faker<RegisterDto> RegisterDtoFaker => new Faker<RegisterDto>()
            .RuleFor(u => u.Username, f => f.Internet.UserName().Replace(".", "_").Replace("-", "_"))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Password, f => "SuperSecurePassword123!");

        public static Faker<RegisterAdminDto> RegisterAdminDtoFaker => new Faker<RegisterAdminDto>()
            .RuleFor(u => u.Username, f => f.Internet.UserName().Replace(".", "_").Replace("-", "_"))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Password, f => "AdminSecurePassword123!")
            .RuleFor(u => u.FirstName, f => f.Name.FirstName())
            .RuleFor(u => u.LastName, f => f.Name.LastName())
            .RuleFor(u => u.PhoneNumber, f => "+52" + f.Random.Number(1000000000, 1999999999).ToString())
            .RuleFor(u => u.Address, f => f.Address.FullAddress())
            .RuleFor(u => u.Rfc, f => "GODE870123H14")
            .RuleFor(u => u.AdminSecretKey, f => "MasterKey");

        public static Faker<User> UserFaker => new Faker<User>()
            .RuleFor(u => u.Id, f => Guid.NewGuid())
            .RuleFor(u => u.Username, f => f.Internet.UserName().Replace(".", "_").Replace("-", "_"))
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.PasswordHash, f => BCrypt.Net.BCrypt.HashPassword("TestPassword123!"))
            .RuleFor(u => u.Role, f => Constants.Roles.Reviewer)
            .RuleFor(u => u.IsSuspended, f => false)
            .RuleFor(u => u.IsDeleted, f => false)
            .RuleFor(u => u.CreatedAt, f => f.Date.Past().ToUniversalTime());
    }
}
