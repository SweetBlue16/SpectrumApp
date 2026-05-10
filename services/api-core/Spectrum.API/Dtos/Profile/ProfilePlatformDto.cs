namespace Spectrum.API.Dtos.Profile
{
    /// <summary>
    /// DTO that represents a platform in user's profile. 
    /// </summary>
    public class ProfilePlatformDto
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;
    }
}