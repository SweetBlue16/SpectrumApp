using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.External;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    /// <summary>
    /// Acts as the gateway for interacting with the external video game catalog (RAWG API).
    /// Provides capabilities to query, filter, and retrieve comprehensive game metadata.
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
        /// <param name="gameService">The service orchestrating external catalog queries and data mapping.</param>

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Retrieves a paginated and filtered catalog of video games from the external provider.
        /// </summary>
        /// <param name="queryDto">The data transfer object containing search terms, genres, platforms, and pagination limits.</param>
        /// <returns>A collection of games matching the specified filter criteria.</returns>
        /// <response code="200">Successfully retrieved the filtered games catalog.</response>
        /// <response code="401">The client lacks valid authentication credentials.</response>
        /// <response code="403">The authenticated user does not have the required role to access this resource.</response>
        /// <response code="503">The external game catalog service is currently unavailable.</response>

        [HttpGet("search")]
        [Authorize(Roles = $"{Constants.Roles.Reviewer},{Constants.Roles.Admin}")]
        [ProducesResponseType(typeof(IEnumerable<RawgGameDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Search([FromQuery] GameQueryDto queryDto)
        {
            var result = await _gameService.SearchGamesAsync(queryDto);
            
            return Ok(result);
        }

        /// <summary>
        /// </summary>

        /// Retrieves comprehensive technical, graphical, and descriptive metadata for a specific video game.
        /// </summary>
        /// <param name="id">The unique identifier assigned by the external game database.</param>
        /// <returns>The detailed metadata profile of the requested game.</returns>
        /// <response code="200">Successfully retrieved the game details.</response>
        /// <response code="401">The client lacks valid authentication credentials.</response>
        /// <response code="404">The requested game ID does not exist in the external catalog.</response>
        /// <response code="503">The external game catalog service is currently unavailable.</response>

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RawgGameDto), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gameDetails = await _gameService.GetGameDetailsAsync(id);
            return Ok(gameDetails);
        }
    }
}
