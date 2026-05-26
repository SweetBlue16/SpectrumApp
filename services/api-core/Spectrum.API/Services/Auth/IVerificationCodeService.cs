using Spectrum.API.Models;

namespace Spectrum.API.Services.Auth
{
    public interface IVerificationCodeService
    {
        Task<string> CreateCodeAsync(VerificationPurpose purpose, string email, Guid? userId);
        Task ConsumeCodeAsync(VerificationPurpose purpose, string email, string code, Guid? userId = null);
        Task<string> VerifyCodeAndCreateSessionAsync(VerificationPurpose purpose, string email, string code, Guid? userId = null);
        Task ConsumeSessionAsync(VerificationPurpose purpose, string email, string verificationToken, Guid? userId = null);
    }
}
