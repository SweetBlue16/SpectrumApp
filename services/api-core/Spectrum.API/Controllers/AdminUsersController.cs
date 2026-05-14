using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Profile;
using Spectrum.API.Services.Profile;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/admin/users")]
    [Produces("application/json")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public class AdminUsersController : ControllerBase
    {
        private readonly IUserModerationService _moderationService;

        public AdminUsersController(IUserModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        [HttpPatch("{userId:guid}/suspend")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SuspendUser(Guid userId, CancellationToken cancellationToken)
        {
            await _moderationService.ToggleSuspensionAsync(userId, suspend: true, cancellationToken);
            return Ok(new { Message = "User has been suspended successfully." });
        }

        [HttpPatch("{userId:guid}/reactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ReactivateUser(Guid userId, CancellationToken cancellationToken)
        {
            await _moderationService.ToggleSuspensionAsync(userId, suspend: false, cancellationToken);
            return Ok(new { Message = "User has been reactivated successfully." });
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<UserModerationDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null, CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1 || pageSize > 50) pageSize = 10;

            var result = await _moderationService.GetUsersForModerationAsync(page, pageSize, search, cancellationToken);

            return Ok(result);
        }
    }
}
