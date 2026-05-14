namespace Spectrum.API.Dtos.Drops
{
    public class EventStatusDto
    {
        public string EventId { get; set; } = string.Empty;
        public int KeysAvailable { get; set; }
        public int KeysTotal { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime EndDate { get; set; }
    }
}
