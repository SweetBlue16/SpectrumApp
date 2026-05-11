namespace Spectrum.API.Dtos.Profile
{
    /// <summary>
    /// DTO that represents a game in the user's profile. 
    /// </summary>
    public class ProfileGameDto
    {
        public String Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? ImageUrl { get; set; }
    }
}