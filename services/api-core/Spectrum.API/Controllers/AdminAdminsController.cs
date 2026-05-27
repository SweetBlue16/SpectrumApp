using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Auth;
using Spectrum.API.Services.Auth;
using Spectrum.API.Utilities;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/admin/admins")]
    [Authorize(Roles = Constants.Roles.Admin)]
    public class AdminAdminsController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AdminAdminsController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost]
        [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status201Created)]
        public async Task<IActionResult> Create([FromBody] RegisterAdminDto dto)
        {
            var response = await _authService.RegisterAdminByAdminAsync(dto);
            return StatusCode(StatusCodes.Status201Created, response);
        }
    }
}
