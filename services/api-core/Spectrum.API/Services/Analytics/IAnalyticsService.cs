using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Dtos.Analytics;
using Spectrum.API.Repositories;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Analytics
{
    public interface IAnalyticsService
    {
        Task<GlobalMetricsDto> GetGlobalMetricsAsync(string period, DateTime? anchorDate, CancellationToken cancellationToken = default);
        Task<WeeklyTrendsDto> GetWeeklyTrendsAsync(CancellationToken cancellationToken = default);
        Task<PagedResult<WeeklyReviewDto>> GetWeeklyClipsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    }

    public class AnalyticsService : IAnalyticsService
    {
        private const int TopGamesLimit = 3;
        private const int ReviewsPerTrendGame = 3;

        private readonly SpectrumDbContext _context;
        private readonly IGameRepository _gameRepository;

        public AnalyticsService(SpectrumDbContext context, IGameRepository gameRepository)
        {
            _context = context;
            _gameRepository = gameRepository;
        }

        public async Task<GlobalMetricsDto> GetGlobalMetricsAsync(
            string period,
            DateTime? anchorDate,
            CancellationToken cancellationToken = default
        )
        {
            var window = ResolveWindow(period, anchorDate ?? DateTime.UtcNow);
            var users = await _context.Users
                .AsNoTracking()
                .Where(user => user.CreatedAt >= window.Start && user.CreatedAt < window.End)
                .Select(user => user.CreatedAt)
                .ToListAsync(cancellationToken);

            var reviews = await _context.Reviews
                .AsNoTracking()
                .Where(review => review.CreatedAt >= window.Start && review.CreatedAt < window.End)
                .Select(review => new { review.CreatedAt, review.GameId })
                .ToListAsync(cancellationToken);

            var topGames = reviews
                .GroupBy(review => review.GameId)
                .Select(group => MapTopGame(group.Key, group.Count()))
                .OrderByDescending(game => game.Count)
                .ThenBy(game => game.GameTitle)
                .Take(5)
                .ToList();

            return new GlobalMetricsDto
            {
                WindowStart = window.Start,
                WindowEnd = window.End,
                NewUsers = BuildSeries(users, period),
                NewReviews = BuildSeries(reviews.Select(review => review.CreatedAt), period),
                MostSearchedGames = topGames
            };
        }

        public async Task<WeeklyTrendsDto> GetWeeklyTrendsAsync(CancellationToken cancellationToken = default)
        {
            var window = ResolveWeeklyWindow(DateTime.UtcNow);
            var topGames = await _context.Reviews
                .AsNoTracking()
                .Where(review => review.CreatedAt >= window.Start && review.CreatedAt < window.End)
                .GroupBy(review => review.GameId)
                .Select(group => new { GameId = group.Key, ReviewsCount = group.Count() })
                .OrderByDescending(group => group.ReviewsCount)
                .ThenBy(group => group.GameId)
                .Take(TopGamesLimit)
                .ToListAsync(cancellationToken);

            if (topGames.Count == 0)
            {
                return new WeeklyTrendsDto
                {
                    WeekStart = window.Start,
                    WeekEnd = window.End,
                    Games = []
                };
            }

            var gameIds = topGames.Select(game => game.GameId).ToArray();
            var reviews = await _context.Reviews
                .AsNoTracking()
                .Include(review => review.User)
                .Where(review => gameIds.Contains(review.GameId) &&
                                 review.CreatedAt >= window.Start &&
                                 review.CreatedAt < window.End)
                .OrderByDescending(review => review.LikesCount)
                .ThenByDescending(review => review.CreatedAt)
                .ToListAsync(cancellationToken);

            var rank = 1;
            var games = topGames
                .Select(game =>
                {
                    var metadata = _gameRepository.GetById(game.GameId);
                    return new WeeklyTrendGameDto
                    {
                        Rank = rank++,
                        GameId = game.GameId,
                        GameTitle = metadata?.Title ?? $"Game {game.GameId}",
                        CoverImageUrl = metadata?.CoverImageUrl ?? string.Empty,
                        ReviewsCount = game.ReviewsCount,
                        Reviews = reviews
                            .Where(review => review.GameId == game.GameId)
                            .Take(ReviewsPerTrendGame)
                            .Select(review => MapWeeklyReview(review, metadata?.Title, metadata?.CoverImageUrl))
                            .ToList()
                    };
                })
                .ToList();

            return new WeeklyTrendsDto
            {
                WeekStart = window.Start,
                WeekEnd = window.End,
                Games = games
            };
        }

        public async Task<PagedResult<WeeklyReviewDto>> GetWeeklyClipsAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default
        )
        {
            var normalizedPage = Math.Max(1, page);
            var normalizedPageSize = Math.Clamp(pageSize, 1, 25);
            var window = ResolveWeeklyWindow(DateTime.UtcNow);

            var query = _context.Reviews
                .AsNoTracking()
                .Include(review => review.User)
                .Where(review => review.CreatedAt >= window.Start &&
                                 review.CreatedAt < window.End &&
                                 review.MediaType != null &&
                                 review.MediaType.ToLower() == "video" &&
                                 review.ImageUrl != null &&
                                 review.ImageUrl != string.Empty);

            var total = await query.CountAsync(cancellationToken);
            var reviews = await query
                .OrderByDescending(review => review.LikesCount)
                .ThenByDescending(review => review.CreatedAt)
                .Skip((normalizedPage - 1) * normalizedPageSize)
                .Take(normalizedPageSize)
                .ToListAsync(cancellationToken);

            return new PagedResult<WeeklyReviewDto>
            {
                Items = reviews.Select(review =>
                {
                    var game = _gameRepository.GetById(review.GameId);
                    return MapWeeklyReview(review, game?.Title, game?.CoverImageUrl);
                }).ToList(),
                TotalCount = total,
                Page = normalizedPage,
                PageSize = normalizedPageSize
            };
        }

        private TopGameMetricDto MapTopGame(int gameId, int count)
        {
            var game = _gameRepository.GetById(gameId);
            return new TopGameMetricDto
            {
                GameId = gameId,
                GameTitle = game?.Title ?? $"Game {gameId}",
                CoverImageUrl = game?.CoverImageUrl ?? string.Empty,
                Count = count
            };
        }

        private static WeeklyReviewDto MapWeeklyReview(Models.Review review, string? gameTitle, string? gameCoverUrl)
        {
            return new WeeklyReviewDto
            {
                ReviewId = review.Id,
                UserId = review.UserId,
                Username = review.User?.Username ?? string.Empty,
                GameId = review.GameId,
                GameTitle = gameTitle ?? $"Game {review.GameId}",
                GameCoverUrl = gameCoverUrl ?? string.Empty,
                Title = review.Title,
                Content = review.Content,
                AttachmentUrl = review.ImageUrl ?? string.Empty,
                AttachmentType = review.MediaType ?? string.Empty,
                LikesCount = review.LikesCount,
                DislikesCount = review.DislikesCount,
                CreatedAt = review.CreatedAt
            };
        }

        private static IReadOnlyList<MetricPointDto> BuildSeries(IEnumerable<DateTime> values, string period)
        {
            return values
                .GroupBy(value => Bucket(value, period))
                .OrderBy(group => group.Key)
                .Select(group => new MetricPointDto
                {
                    Date = group.Key,
                    Label = BuildLabel(group.Key, period),
                    Count = group.Count()
                })
                .ToList();
        }

        private static DateTime Bucket(DateTime value, string period)
        {
            var utcValue = value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime();

            return period.Equals("day", StringComparison.OrdinalIgnoreCase)
                ? new DateTime(utcValue.Year, utcValue.Month, utcValue.Day, utcValue.Hour, 0, 0, DateTimeKind.Utc)
                : utcValue.Date;
        }

        private static string BuildLabel(DateTime value, string period)
        {
            return period.Equals("day", StringComparison.OrdinalIgnoreCase)
                ? value.ToString("HH:00")
                : value.ToString("yyyy-MM-dd");
        }

        private static (DateTime Start, DateTime End) ResolveWindow(string period, DateTime anchorDate)
        {
            var anchorUtc = anchorDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(anchorDate, DateTimeKind.Utc)
                : anchorDate.ToUniversalTime();

            return period.ToLowerInvariant() switch
            {
                "day" => (anchorUtc.Date, anchorUtc.Date.AddDays(1)),
                "month" => (new DateTime(anchorUtc.Year, anchorUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(anchorUtc.Year, anchorUtc.Month, 1, 0, 0, 0, DateTimeKind.Utc).AddMonths(1)),
                _ => ResolveWeeklyWindow(anchorUtc)
            };
        }

        private static (DateTime Start, DateTime End) ResolveWeeklyWindow(DateTime anchorDate)
        {
            var anchorUtc = anchorDate.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(anchorDate, DateTimeKind.Utc)
                : anchorDate.ToUniversalTime();
            var daysFromMonday = ((int)anchorUtc.DayOfWeek + 6) % 7;
            var start = anchorUtc.Date.AddDays(-daysFromMonday);

            return (start, start.AddDays(7));
        }
    }
}
