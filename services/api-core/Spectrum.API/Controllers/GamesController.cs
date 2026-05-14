using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.External;
using Spectrum.API.Models;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    /// <summary>
    /// Acts as the gateway for interacting with the internal video game catalog.
    /// Provides capabilities to query, filter, and retrieve game metadata from the memory cache.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    [Produces("application/json")]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        /// <summary>
        /// Initializes a new instance of the <see cref="GamesController"/> class.
        /// </summary>
        /// <param name="gameService">The service orchestrating catalog queries and data mapping.</param>
        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Retrieves a paginated and filtered catalog of video games from the internal memory cache.
        /// </summary>
        /// <param name="queryDto">The data transfer object containing search terms, genres, platforms, and pagination limits.</param>
        /// <returns>A collection of games matching the specified filter criteria.</returns>
        /// <response code="200">Successfully retrieved the filtered games catalog from cache.</response>
        /// <response code="401">The client lacks valid authentication credentials.</response>
        /// <response code="403">The authenticated user does not have the required role to access this resource.</response>
        [HttpGet("search")]
        [Authorize(Roles = $"{Constants.Roles.Reviewer},{Constants.Roles.Admin}")]
        [ProducesResponseType(typeof(PagedResult<Game>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Search([FromQuery] GameQueryDto queryDto)
        {
            var result = await _gameService.SearchGamesAsync(queryDto);

            return Ok(result);
        }

        /// <summary>
        /// Retrieves metadata for a specific video game from the internal catalog.
        /// </summary>
        /// <param name="id">The unique RAWG identifier of the game.</param>
        /// <returns>The metadata profile of the requested game.</returns>
        /// <response code="200">Successfully retrieved the game details.</response>
        /// <response code="401">The client lacks valid authentication credentials.</response>
        /// <response code="404">The requested game ID does not exist in the local catalog.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Game), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gameDetails = await _gameService.GetGameDetailsAsync(id);
            return Ok(gameDetails);
        }
    }
}