namespace Spectrum.API.Dtos.Analytics
{
    public class MetricPointDto
    {
        public DateTime Date { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
