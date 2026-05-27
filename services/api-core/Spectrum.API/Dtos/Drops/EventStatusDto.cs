namespace Spectrum.API.Dtos.Drops
{
    public class EventStatusDto
    {
        public string EventId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string GameTitle { get; set; } = string.Empty;
        public int? RawgGameId { get; set; }
        public string Platform { get; set; } = string.Empty;
        public DateTime StartAt { get; set; }
        public DateTime JoinDeadlineAt { get; set; }
        public DateTime RevealAt { get; set; }
        public DateTime EndAt { get; set; }
        public int TotalSlots { get; set; }
        public int AvailableSlots { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PublicChallengeCode { get; set; } = string.Empty;
        public string CreatedByAdminId { get; set; } = string.Empty;
        public string? WinnerUserId { get; set; }
        public string? WinnerUsername { get; set; }
        public DateTime? FinishedAt { get; set; }
        public DateTime? RewardSentAt { get; set; }
        public string RewardDeliveryStatus { get; set; } = "PENDING";
        public int ParticipantsCount { get; set; }
        public int KeysAvailable => AvailableSlots;
        public int KeysTotal => TotalSlots;
        public DateTime EndDate => EndAt;
    }
}
