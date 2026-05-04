using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spectrum.API.Dtos.Reviews;
using Spectrum.API.Dtos.Votes;
using Spectrum.API.Services.Reviews;
using Spectrum.API.Services.Votes;
using System.Security.Claims;

namespace Spectrum.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewsController : ControllerBase
    {
        private readonly IReviewService _reviewService;
        private readonly IVoteService _voteService;

        public ReviewsController(IReviewService reviewService, IVoteService voteService)
        {
            _reviewService = reviewService;
            _voteService = voteService;
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult<ReviewResponseDto>> Create([FromBody] CreateReviewDto dto)
        {
            var userId = GetCurrentUserId();

            var review = await _reviewService.CreateAsync(dto, userId);

            return CreatedAtAction(
                nameof(GetById),
                new { reviewId = review.Id },
                review
            );
        }

        [HttpPut("{reviewId:guid}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid reviewId, [FromBody] UpdateReviewDto dto)
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            await _reviewService.UpdateAsync(reviewId, dto, userId, isAdmin);

            return NoContent();
        }

        [HttpDelete("{reviewId:guid}")]
        [Authorize]
        public async Task<IActionResult> Delete(Guid reviewId)
        {
            var userId = GetCurrentUserId();
            var isAdmin = IsCurrentUserAdmin();

            await _reviewService.DeleteAsync(reviewId, userId, isAdmin);

            return NoContent();
        }

        [HttpGet("{reviewId:guid}")]
        public async Task<ActionResult<ReviewResponseDto>> GetById(Guid reviewId)
        {
            var review = await _reviewService.GetByIdAsync(reviewId);

            return Ok(review);
        }

        [HttpGet("game/{gameId:int}")]
        public async Task<ActionResult<IReadOnlyList<ReviewResponseDto>>> GetByGame(int gameId)
        {
            var reviews = await _reviewService.GetByGameIdAsync(gameId);

            return Ok(reviews);
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<IReadOnlyList<ReviewResponseDto>>> GetMine()
        {
            var userId = GetCurrentUserId();

            var reviews = await _reviewService.GetByUserIdAsync(userId);

            return Ok(reviews);
        }

        [HttpPost("{reviewId:guid}/vote")]
        [Authorize]
        public async Task<ActionResult<VoteResultDto>> Vote(
            Guid reviewId,
            [FromBody] CastReviewVoteDto dto
        )
        {
            var userId = GetCurrentUserId();

            var result = await _voteService.CastReviewVoteAsync(
                reviewId,
                userId,
                dto.IsPositive
            );

            return Ok(result);
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim =
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                User.FindFirst("sub")?.Value ??
                User.FindFirst("userId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                throw new UnauthorizedAccessException("No se encontró el identificador del usuario autenticado.");
            }

            return Guid.Parse(userIdClaim);
        }

        private bool IsCurrentUserAdmin()
        {
            return User.IsInRole("Admin") ||
                   User.IsInRole("ADMIN") ||
                   User.HasClaim(ClaimTypes.Role, "Admin") ||
                   User.HasClaim(ClaimTypes.Role, "ADMIN") ||
                   User.HasClaim("role", "Admin") ||
                   User.HasClaim("role", "ADMIN");
        }
    }
}