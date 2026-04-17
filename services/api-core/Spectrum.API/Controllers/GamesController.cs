using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Services.External;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GamesController : ControllerBase
    {
        private readonly IGameIntegrationService _gameIntegrationService;

        public GamesController(IGameIntegrationService gameIntegrationService)
        {
            _gameIntegrationService = gameIntegrationService;
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(Array.Empty<object>());
            }
            var games = await _gameIntegrationService.SearchGamesAsync(query);
            return Ok(games);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gameDetails = await _gameIntegrationService.GetGameDetailsAsync(id);
            return Ok(gameDetails);
        }
    }
}
