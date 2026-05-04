namespace Spectrum.API.Dtos.Votes
{
    public class VoteResultDto
    {
        public bool Success { get; set; }
        public int UpdatedLikes { get; set; }
        public int UpdatedDislikes { get; set; }
    }
}
