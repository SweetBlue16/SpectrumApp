namespace Spectrum.API.Dtos.Drops
{
    public class WonKeyDto
    {
        public string EventId { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public string AccessKeyCode { get; set; } = string.Empty;
        public DateTime ClaimedAt { get; set; }
    }
}
