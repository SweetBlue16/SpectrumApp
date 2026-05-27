using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Analytics;
using Spectrum.API.Services.Analytics;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/clips/weekly")]
    [Authorize]
    public class WeeklyClipsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public WeeklyClipsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<WeeklyReviewDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetWeeklyClips(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default
        )
        {
            var clips = await _analyticsService.GetWeeklyClipsAsync(page, pageSize, cancellationToken);
            return Ok(clips);
        }
    }
}
