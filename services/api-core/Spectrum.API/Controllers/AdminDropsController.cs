using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Drops;
using Spectrum.API.Services.Drops;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/admin/drops")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public class AdminDropsController : ControllerBase
    {
        private readonly IDropsService _dropService;

        public AdminDropsController(IDropsService dropService)
        {
            _dropService = dropService;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateDropEventDto dto, CancellationToken cancellationToken)
        {
            await _dropService.CreateEventAsync(dto, cancellationToken);
            return Ok(new { Message = "Sorteo creado exitosamente." });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] UpdateDropEventDto dto, CancellationToken cancellationToken)
        {
            await _dropService.UpdateEventAsync(id, dto, cancellationToken);
            return Ok(new { Message = "Sorteo actualizado exitosamente." });
        }
    }
}
