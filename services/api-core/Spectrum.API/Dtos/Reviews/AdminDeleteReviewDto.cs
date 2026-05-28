using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Reviews
{
    public class AdminDeleteReviewDto
    {
        [Required]
        [MinLength(10)]
        [MaxLength(300)]
        public string Reason { get; set; } = string.Empty;
    }
}
