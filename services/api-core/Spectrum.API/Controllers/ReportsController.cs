using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Reports;
using Spectrum.API.Services.Reports;
using Spectrum.API.Utilities;
using System.Security.Claims;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpPost]
        [Authorize]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SubmitReport([FromBody] CreateReportDto dto, CancellationToken cancellationToken)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            await _reportService.SubmitReportAsync(userId, dto, cancellationToken);

            return Ok(new { Message = "Report submitted successfully." });
        }

        [HttpGet]
        [Authorize(Roles = Constants.Roles.Admin)]
        [ProducesResponseType(typeof(IEnumerable<ReportDetailsDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetReports([FromQuery] string status = "PENDING", CancellationToken cancellationToken = default)
        {
            var reports = await _reportService.GetReportsByStatusAsync(status.ToUpper(), cancellationToken);
            return Ok(reports);
        }

        [HttpPatch("{reportId}")]
        [Authorize(Roles = Constants.Roles.Admin)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> ResolveReport(string reportId, [FromBody] UpdateReportStatusDto dto, CancellationToken cancellationToken)
        {
            var moderatorIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(moderatorIdStr, out var moderatorId)) return Unauthorized();

            await _reportService.UpdateReportStatusAsync(reportId, moderatorId, dto, cancellationToken);

            return Ok(new { Message = $"Report status updated to {dto.NewStatus}." });
        }
    }
}
