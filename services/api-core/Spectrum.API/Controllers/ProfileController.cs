using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Profile;
using Spectrum.API.Repositories;
using System.Security.Claims;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public ProfileController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
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

            var user = await _userRepository.GetUserByEmailAsync(email);

            if (user == null)
                return NotFound();

            return Ok(new UserProfileDto
            {
                Username = user.Username, 
                Email = user.Email,       
                ProfilePicture = user.ProfilePicture 
            });
        }
    }
}