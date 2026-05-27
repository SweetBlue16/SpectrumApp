using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Seed;
using Spectrum.API.Services.Seeding;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/dev/seed")]
    public class DevelopmentSeedController : ControllerBase
    {
        private readonly IDemoSeedService _demoSeedService;
        private readonly IWebHostEnvironment _environment;

        public DevelopmentSeedController(IDemoSeedService demoSeedService, IWebHostEnvironment environment)
        {
            _demoSeedService = demoSeedService;
            _environment = environment;
        }

        [HttpPost("demo")]
        [ProducesResponseType(typeof(DemoSeedResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SeedDemo(CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            return Ok(await _demoSeedService.SeedAsync(cancellationToken));
        }

        [HttpDelete("demo")]
        [ProducesResponseType(typeof(DemoSeedResultDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CleanupDemo(CancellationToken cancellationToken)
        {
            if (!_environment.IsDevelopment())
            {
                return NotFound();
            }

            return Ok(await _demoSeedService.CleanupAsync(cancellationToken));
        }
    }
}
