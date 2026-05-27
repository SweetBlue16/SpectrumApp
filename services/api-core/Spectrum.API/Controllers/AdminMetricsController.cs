using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Analytics;
using Spectrum.API.Services.Analytics;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/admin/metrics")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public class AdminMetricsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AdminMetricsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("global")]
        [ProducesResponseType(typeof(GlobalMetricsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetGlobalMetrics(
            [FromQuery] string period = "week",
            [FromQuery] DateTime? anchorDate = null,
            CancellationToken cancellationToken = default
        )
        {
            var metrics = await _analyticsService.GetGlobalMetricsAsync(period, anchorDate, cancellationToken);
            return Ok(metrics);
        }
    }
}
