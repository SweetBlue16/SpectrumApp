using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Services.Drops;
using Spectrum.API.Utilities;
using System.Security.Claims;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DropsController : ControllerBase
    {
        private readonly IDropsService _dropsService;

        public DropsController(IDropsService dropsService)
        {
            _dropsService = dropsService;
        }

        [HttpGet("event/{eventId}")]
        public async Task<IActionResult> GetStatus(string eventId, CancellationToken cancellationToken)
        {
            var status = await _dropsService.GetEventStatusAsync(eventId, cancellationToken);
            return Ok(status);
        }

        [HttpPost("claim/{eventId}")]
        public async Task<IActionResult> Claim(string eventId, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var result = await _dropsService.ClaimAccessKeyAsync(userId, eventId, cancellationToken);

            if (result == null)
            {
                return BadRequest(new { Message = Constants.ErrorMessages.CouldNotClaimKey });
            }

            return Ok(result);
        }

        [HttpGet("my-keys")]
        public async Task<IActionResult> GetMyKeys(CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var keys = await _dropsService.GetUserWonKeysAsync(userId, ct);
            return Ok(keys);
        }
    }
}
