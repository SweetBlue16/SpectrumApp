namespace Spectrum.API.Dtos.Drops
{
    public class ClaimDropResultDto
    {
        public bool Success { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string? WinnerUserId { get; set; }
        public string? WinnerUsername { get; set; }
        public DateTime? ClaimedAt { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
