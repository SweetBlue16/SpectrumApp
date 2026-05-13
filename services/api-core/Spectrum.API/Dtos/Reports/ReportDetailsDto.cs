namespace Spectrum.API.Dtos.Reports
{
    public class ReportDetailsDto
    {
        public string ReportId { get; set; } = string.Empty;
        public Guid ReporterId { get; set; }
        public Guid TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
    }
}
