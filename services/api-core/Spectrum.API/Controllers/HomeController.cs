using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Home;
using Spectrum.API.Services.Home;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/home")]
    [Authorize]
    public class HomeController : ControllerBase
    {
        private readonly IHomeDashboardService _homeDashboardService;

        public HomeController(IHomeDashboardService homeDashboardService)
        {
            _homeDashboardService = homeDashboardService;
        }

        [HttpGet("dashboard")]
        [ProducesResponseType(typeof(HomeDashboardDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetDashboard(CancellationToken cancellationToken)
        {
            return Ok(await _homeDashboardService.GetDashboardAsync(cancellationToken));
        }
    }
}
