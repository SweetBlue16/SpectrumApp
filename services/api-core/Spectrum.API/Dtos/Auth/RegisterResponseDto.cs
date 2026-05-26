namespace Spectrum.API.Dtos.Auth
{
    public class RegisterResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public bool RequiresVerification { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
