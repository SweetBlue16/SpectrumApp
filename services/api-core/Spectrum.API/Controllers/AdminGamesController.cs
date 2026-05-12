using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Services.External;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    /// <summary>
    /// Provides administrative operations for managing the video game catalog.
    /// Access is restricted to users with the Administrator role.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = Constants.Roles.Admin)]
    [Produces("application/json")]
    public class AdminGamesController : ControllerBase
    {
        private readonly IRawgSyncService _rawgSyncService;
        private readonly ILogger<AdminGamesController> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminGamesController"/> class.
        /// </summary>
        /// <param name="rawgSyncService">Service responsible for RAWG data synchronization.</param>
        /// <param name="logger">The logger instance for administrative events.</param>
        public AdminGamesController(IRawgSyncService rawgSyncService, ILogger<AdminGamesController> logger)
        {
            _rawgSyncService = rawgSyncService;
            _logger = logger;
        }

        /// <summary>
        /// Triggers a full synchronization of the external RAWG catalog to the local JSON snapshot.
        /// This process runs in the background due to its long-running nature.
        /// </summary>
        /// <returns>An object indicating that the synchronization process has been initiated.</returns>
        /// <response code="202">The synchronization request has been accepted and is running in the background.</response>
        /// <response code="401">The client lacks valid authentication credentials.</response>
        /// <response code="403">The authenticated user is not an Administrator.</response>
        [HttpPost("sync-catalog")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status403Forbidden)]
        public IActionResult SyncCatalog([FromQuery] bool fullSync = false)
        {
            _logger.LogInformation("Sync initiated. FullSync: {Status}", fullSync);

            _ = Task.Run(async () =>
            {
                try
                {
                    await _rawgSyncService.SyncCatalogAsync(fullSync);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Sync failed");
                }
            });

            return Accepted(new { Message = "Process started", Mode = fullSync ? "Full" : "Incremental" });
        }
    }
}