namespace Spectrum.API.Services.Drops
{
    public interface IRewardDeliveryService
    {
        Task SendRewardAsync(
            string recipientEmail,
            string eventTitle,
            string rewardCode,
            CancellationToken cancellationToken = default
        );
    }

    public class SafeLogRewardDeliveryService : IRewardDeliveryService
    {
        private readonly ILogger<SafeLogRewardDeliveryService> _logger;

        public SafeLogRewardDeliveryService(ILogger<SafeLogRewardDeliveryService> logger)
        {
            _logger = logger;
        }

        public Task SendRewardAsync(
            string recipientEmail,
            string eventTitle,
            string rewardCode,
            CancellationToken cancellationToken = default
        )
        {
            var recipientHash = Convert.ToHexString(
                System.Security.Cryptography.SHA256.HashData(
                    System.Text.Encoding.UTF8.GetBytes(recipientEmail.Trim().ToLowerInvariant())
                )
            )[..12];

            _logger.LogInformation(
                "Reward delivery queued for event {EventTitle} to recipient hash {RecipientHash}.",
                eventTitle,
                recipientHash
            );

            return Task.CompletedTask;
        }
    }
}
