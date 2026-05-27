using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Analytics;
using Spectrum.API.Services.Analytics;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/trends")]
    [Authorize]
    public class TrendsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public TrendsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("weekly")]
        [ProducesResponseType(typeof(WeeklyTrendsDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWeekly(CancellationToken cancellationToken)
        {
            var trends = await _analyticsService.GetWeeklyTrendsAsync(cancellationToken);
            return Ok(trends);
        }
    }
}
