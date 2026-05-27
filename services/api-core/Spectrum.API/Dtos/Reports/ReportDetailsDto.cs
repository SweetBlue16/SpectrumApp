namespace Spectrum.API.Dtos.Reports
{
    public class ReportDetailsDto
    {
        public string Id { get; set; } = string.Empty;
        public string ReportId { get; set; } = string.Empty;
        public Guid ReporterId { get; set; }
        public Guid TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime ReportedAt { get; set; }
        public string ReporterUsername { get; set; } = string.Empty;
        public string? TargetContentSnippet { get; set; }
        public string? AdminNotes { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolutionAction { get; set; }
    }
}
