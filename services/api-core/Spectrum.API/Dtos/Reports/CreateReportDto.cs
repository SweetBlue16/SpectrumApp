using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reports
{
    public class CreateReportDto
    {
        public required Guid TargetId { get; set; }

        [Required]
        [RegularExpression("^(REVIEW|COMMENT)$", ErrorMessage = "TargetType must be REVIEW or COMMENT.")]
        public string TargetType { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;
    }
}
