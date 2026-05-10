namespace Spectrum.API.Dtos.Profile
{
    /// <summary>
    /// DTO that represents a game in the user's profile. 
    /// </summary>
    public class ProfileGameDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}