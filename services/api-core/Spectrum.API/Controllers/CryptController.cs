using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Analytics;
using Spectrum.API.Services.Analytics;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/crypt")]
    [Authorize]
    public class CryptController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public CryptController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(CryptDashboardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            return Ok(await _analyticsService.GetCryptDashboardAsync(cancellationToken));
        }
    }
}
