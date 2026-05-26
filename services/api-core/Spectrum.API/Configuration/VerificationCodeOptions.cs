namespace Spectrum.API.Configuration
{
    public class VerificationCodeOptions
    {
        public const string SectionName = "VerificationCodes";

        public int CodeLength { get; set; } = 6;
        public int ExpirationMinutes { get; set; } = 10;
        public int MaxAttempts { get; set; } = 5;
        public int ResendCooldownSeconds { get; set; } = 60;
    }
}
