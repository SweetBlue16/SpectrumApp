using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reports
{
    public class UpdateReportStatusDto
    {
        public string NewStatus { get; set; } = string.Empty;

        public string ResolutionNotes { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;

        public string AdminNotes { get; set; } = string.Empty;
    }
}
