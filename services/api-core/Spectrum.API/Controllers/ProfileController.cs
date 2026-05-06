using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Profile;
using Spectrum.API.Services.Profile;
using System.Security.Claims;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService profileService; // Sin guion bajo

        public ProfileController(IProfileService profileService)
        {
            this.profileService = profileService;
        }

        /// <summary>
        /// Gets the profile information of the currently authenticated user.
        /// </summary>
        [HttpGet("me")]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(email))
                return Unauthorized();

            var profile = await profileService.GetUserProfileByEmailAsync(email);

            if (profile == null)
                return NotFound();

            return Ok(profile);
        }
    }
}