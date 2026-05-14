using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reports
{
    public class UpdateReportStatusDto
    {
        [Required]
        [RegularExpression("^(RESOLVED|DISMISSED)$", ErrorMessage = "Status must be RESOLVED or DISMISSED.")]
        public string NewStatus { get; set; } = string.Empty;

        public string ResolutionNotes { get; set; } = string.Empty;
    }
}
