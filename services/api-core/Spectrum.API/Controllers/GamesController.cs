using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.External;
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
        [Authorize(Roles = "REVIEWER,ADMIN")]
        public async Task<IActionResult> Search([FromQuery] GameQueyDto queryDto)
        {
            var result = await _gameIntegrationService.SearchGamesAsync(queryDto);
            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gameDetails = await _gameIntegrationService.GetGameDetailsAsync(id);
            return Ok(gameDetails);
        }
    }
}
