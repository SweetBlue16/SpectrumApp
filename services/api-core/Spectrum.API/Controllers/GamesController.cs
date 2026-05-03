using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.External;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities; 

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class GamesController : ControllerBase
    {
        private readonly IGameService _gameService;

        public GamesController(IGameService gameService)
        {
            _gameService = gameService;
        }

        /// <summary>
        /// Obtiene una lista paginada de juegos.
        /// </summary>
        /// <param name="queryDto">Parámetros de búsqueda, filtrado y paginación.</param>
        /// <returns>Un objeto PagedResult con la lista de juegos y metadatos de paginación.</returns>
        [HttpGet("search")]
        [Authorize(Roles = "REVIEWER,ADMIN")]
        public async Task<IActionResult> Search([FromQuery] GameQueyDto queryDto)
        {
            var result = await _gameService.SearchGamesAsync(queryDto);
            
            return Ok(result);
        }

        /// <summary>
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDetails(int id)
        {
            var gameDetails = await _gameService.GetGameDetailsAsync(id);
            return Ok(gameDetails);
        }
    }
}
