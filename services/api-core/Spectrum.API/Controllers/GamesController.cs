using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.External;
using Spectrum.API.Services.External;

namespace Spectrum.API.Controllers
{
    /// <summary>
    /// Controller that handles interactions with the external video game catalog.
    /// Provides search and detailed information retrieval capabilities.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GamesController : ControllerBase
    {
        private readonly IGameIntegrationService _gameIntegrationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamesController"/> class.
        /// </summary>
        /// <param name="gameIntegrationService">Service for external API integration.</param>
        public GamesController(IGameIntegrationService gameIntegrationService)
        {
            _gameIntegrationService = gameIntegrationService;
        }

        /// <summary>
        /// Retrieves a filtered list of video games from the external provider.
        /// </summary>
        /// <param name="queryDto">Filter criteria including search terms, genres, and platforms.</param>
        /// <returns>A paginated list of games matching the criteria.</returns>
        /// <response code="200">Returns the matching games catalog.</response>
        /// <response code="401">If the user is not authenticated.</response>
        /// <response code="503">If the external RAWG service is unavailable.</response>
        [HttpGet("search")]
        [Authorize(Roles = "REVIEWER,ADMIN")]
        public async Task<IActionResult> Search([FromQuery] GameQueyDto queryDto)
        {
            var result = await _gameIntegrationService.SearchGamesAsync(queryDto);
            return Ok(result);
        }

        /// <summary>
        /// Gets full technical and descriptive details for a specific game.
        /// </summary>
        /// <param name="id">The external provider's unique game ID.</param>
        /// <returns>Detailed game information.</returns>
        /// <response code="200">Returns the full game details.</response>
        /// <response code="404">If the game was not found in the external catalog.</response>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gameDetails = await _gameIntegrationService.GetGameDetailsAsync(id);
            return Ok(gameDetails);
        }
    }
}
