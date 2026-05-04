using System.ComponentModel.DataAnnotations;

namespace Spectrum.API.Dtos.Votes
{
    public class CastReviewVoteDto
    {
        [Required]
        public bool IsPositive { get; set; }
    }
}
