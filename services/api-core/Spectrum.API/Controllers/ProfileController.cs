using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spectrum.API.Dtos.Auth;
using Spectrum.API.Dtos.Profile;
using Spectrum.API.Services.Profile;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Spectrum.API.Controllers
{
    /// <summary>
    /// Controller responsible for managing user profile operations.
    /// Relies on GlobalExceptionHandler to map business exceptions to ProblemDetails.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService profileService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileController"/> class.
        /// </summary>
        /// <param name="profileService">The service used for profile business logic.</param>
        public ProfileController(IProfileService profileService)
        {
            this.profileService = profileService;
        }

        /// <summary>
        /// Retrieves the profile information for the currently authenticated user.
        /// </summary>
        /// <returns>A <see cref="UserProfileDto"/> containing the user's detailed data.</returns>
        /// <response code="200">Successfully retrieved the user profile.</response>
        /// <response code="401">The user is not authenticated or the token is invalid.</response>
        /// <response code="404">The requested user profile was not found in the system.</response>
        [HttpGet("me")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetMyProfile()
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var profile = await profileService.GetUserProfileAsync(userId);

            return Ok(profile);
        }

        [HttpGet("users/{userId:guid}")]
        [ProducesResponseType(typeof(UserProfileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserProfileDto>> GetPublicProfile(Guid userId)
        {
            var profile = await profileService.GetPublicUserProfileAsync(userId);
            return Ok(profile);
        }

        [HttpPost("users/{userId:guid}/block")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> BlockUser(Guid userId, [FromBody] BlockUserDto dto)
        {
            if (!TryGetAuthenticatedUserId(out var blockerUserId))
            {
                return Unauthorized();
            }

            await profileService.BlockUserAsync(blockerUserId, userId, dto);
            return NoContent();
        }

        /// <summary>
        /// Updates the profile information for the authenticated user.
        /// </summary>
        /// <param name="profileDto">The updated profile data.</param>
        /// <returns>A 204 No Content response if successful.</returns>
        /// <response code="204">Successfully updated the user profile.</response>
        /// <response code="400">The provided profile data is invalid (validation error).</response>
        /// <response code="401">The user is not authenticated or the token is invalid.</response>
        /// <response code="404">The user profile to update was not found.</response>
        [HttpPut("me")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProfile([FromBody] UserProfileDto profileDto)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }
            await profileService.UpdateUserProfileAsync(userId, profileDto);

            return NoContent();
        }

        /// <summary>
        /// Updates the password for the authenticated user after verifying the current one.
        /// </summary>
        /// <param name="passwordDto">The data containing current and new passwords.</param>
        /// <returns>A 204 No Content response if successful.</returns>
        /// <response code="204">Successfully updated the password.</response>
        /// <response code="400">The provided password data is invalid.</response>
        /// <response code="401">The user is not authenticated or current password is incorrect.</response>
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto passwordDto)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            await profileService.ChangePasswordAsync(userId, passwordDto);

            return NoContent();
        }

        [HttpPost("me/password/change/request-code")]
        [EnableRateLimiting("SensitiveAuth")]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RequestPasswordChangeCode()
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized();
            }

            await profileService.RequestPasswordChangeCodeAsync(userId);
            return Ok(new MessageResponseDto { Message = "verificationCodeSent" });
        }

        [HttpPost("me/password/change/verify-code")]
        [EnableRateLimiting("SensitiveAuth")]
        [ProducesResponseType(typeof(PasswordCodeVerifiedDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> VerifyPasswordChangeCode([FromBody] VerifyPasswordChangeCodeDto verifyDto)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized();
            }

            var verificationToken = await profileService.VerifyPasswordChangeCodeAsync(userId, verifyDto);
            return Ok(new PasswordCodeVerifiedDto
            {
                VerificationToken = verificationToken,
                Message = "verificationCodeVerified"
            });
        }

        [HttpPost("me/password/change/confirm")]
        [EnableRateLimiting("SensitiveAuth")]
        [ProducesResponseType(typeof(MessageResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ConfirmPasswordChange([FromBody] ConfirmPasswordChangeDto confirmDto)
        {
            if (!TryGetAuthenticatedUserId(out var userId))
            {
                return Unauthorized();
            }

            await profileService.ConfirmPasswordChangeAsync(userId, confirmDto);
            return Ok(new MessageResponseDto { Message = "passwordUpdated" });
        }

        /// <summary>
        /// Updates the authenticated user's profile picture using AWS S3 storage.
        /// </summary>
        /// <param name="file">The image file payload (JPG/PNG).</param>
        /// <returns>An object containing the secure public URL of the uploaded avatar.</returns>
        /// <response code="200">The profile picture was successfully uploaded and updated.</response>
        /// <response code="401">The user session token is missing or invalid.</response>
        /// <response code="400">The image failed payload size or format validations.</response>
        [HttpPut("avatar")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateAvatar(IFormFile file)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var newAvatarUrl = await profileService.UpdateAvatarAsync(userId, file);
            return Ok(new { avatarUrl = newAvatarUrl });
        }

        private bool TryGetAuthenticatedUserId(out Guid userId)
        {
            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                             ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return Guid.TryParse(userIdClaim, out userId);
        }
    }
}
