namespace Spectrum.API.Services.Email
{
    public interface IEmailService
    {
        Task SendRegistrationVerificationAsync(string email, string code);
        Task SendPasswordResetAsync(string email, string code);
        Task SendPasswordChangeAsync(string email, string code);
        Task SendRewardAsync(string email, string eventTitle, string rewardCode);
    }
}
