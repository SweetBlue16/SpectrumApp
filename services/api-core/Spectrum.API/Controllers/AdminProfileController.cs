using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Admin;
using Spectrum.API.Services.Admin;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/admin/profile")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public class AdminProfileController : ControllerBase
    {
        private readonly IAdminProfileService _profileService;

        public AdminProfileController(IAdminProfileService profileService)
        {
            _profileService = profileService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AdminProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Get(CancellationToken cancellationToken)
        {
            var profile = await _profileService.GetProfileAsync(GetCurrentAdminId(), cancellationToken);
            return Ok(profile);
        }

        [HttpPut]
        [ProducesResponseType(typeof(AdminProfileDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Update([FromBody] UpdateAdminProfileDto dto, CancellationToken cancellationToken)
        {
            var profile = await _profileService.UpdateProfileAsync(GetCurrentAdminId(), dto, cancellationToken);
            return Ok(profile);
        }

        private Guid GetCurrentAdminId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub")
                ?? User.FindFirstValue("userId");

            return Guid.TryParse(userId, out var parsedUserId)
                ? parsedUserId
                : throw new UnauthorizedAccessException();
        }
    }
}
