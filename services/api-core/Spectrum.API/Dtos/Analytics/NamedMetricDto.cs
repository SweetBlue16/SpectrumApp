namespace Spectrum.API.Dtos.Analytics
{
    public class NamedMetricDto
    {
        public string Id { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Score { get; set; }
        public string? ImageUrl { get; set; }
    }
}
