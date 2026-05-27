using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Search;
using Spectrum.API.Services.Search;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/search")]
    [Authorize]
    public class SearchController : ControllerBase
    {
        private readonly IGlobalSearchService _globalSearchService;

        public SearchController(IGlobalSearchService globalSearchService)
        {
            _globalSearchService = globalSearchService;
        }

        [HttpGet("global")]
        [ProducesResponseType(typeof(GlobalSearchResultDto), StatusCodes.Status200OK)]
        public async Task<IActionResult> Search([FromQuery] string q, CancellationToken cancellationToken)
        {
            return Ok(await _globalSearchService.SearchAsync(q, cancellationToken));
        }
    }
}
