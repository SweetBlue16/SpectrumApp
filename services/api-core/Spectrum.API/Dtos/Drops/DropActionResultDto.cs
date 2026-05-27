namespace Spectrum.API.Dtos.Drops
{
    public class DropActionResultDto
    {
        public bool Success { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
