namespace Spectrum.API.Dtos.Drops
{
    public class CreateDropEventDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public int? RawgGameId { get; set; }
        public string Platform { get; set; } = string.Empty;
        public required DateTime StartAt { get; set; }
        public required DateTime JoinDeadlineAt { get; set; }
        public required DateTime RevealAt { get; set; }
        public required DateTime EndAt { get; set; }
        public int TotalSlots { get; set; }
        public string PublicChallengeCode { get; set; } = string.Empty;
        public bool PublishNow { get; set; }
    }
}
