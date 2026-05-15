namespace Spectrum.API.Dtos.Drops
{
    public class UpdateDropEventDto
    {
        public string GameTitle { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public required DateTime EndDate { get; set; }
        public string Status { get; set; } = "ACTIVE";
    }
}
