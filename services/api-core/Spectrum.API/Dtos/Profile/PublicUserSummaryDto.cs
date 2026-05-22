namespace Spectrum.API.Dtos.Profile
{
    public class PublicUserSummaryDto
    {
        public Guid Id { get; set; }

        public string Username { get; set; } = string.Empty;

        public string ProfilePicture { get; set; } = string.Empty;
    }
}
