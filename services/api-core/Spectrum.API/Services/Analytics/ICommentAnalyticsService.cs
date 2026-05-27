using Grpc.Core;
using Spectrum.API.Grpc.Social;

namespace Spectrum.API.Services.Analytics
{
    public interface ICommentAnalyticsService
    {
        Task<IReadOnlyDictionary<Guid, int>> GetCommentCountsAsync(
            IEnumerable<Guid> reviewIds,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default
        );
    }

    public class CommentAnalyticsService : ICommentAnalyticsService
    {
        private readonly CommentService.CommentServiceClient _commentServiceClient;
        private readonly ILogger<CommentAnalyticsService> _logger;

        public CommentAnalyticsService(
            CommentService.CommentServiceClient commentServiceClient,
            ILogger<CommentAnalyticsService> logger
        )
        {
            _commentServiceClient = commentServiceClient;
            _logger = logger;
        }

        public async Task<IReadOnlyDictionary<Guid, int>> GetCommentCountsAsync(
            IEnumerable<Guid> reviewIds,
            DateTime? from = null,
            DateTime? to = null,
            CancellationToken cancellationToken = default
        )
        {
            var ids = reviewIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return new Dictionary<Guid, int>();
            }

            var request = new GetCommentCountsRequest
            {
                From = from.HasValue ? ToUnixMilliseconds(from.Value) : 0,
                To = to.HasValue ? ToUnixMilliseconds(to.Value) : 0
            };
            request.ReviewIds.AddRange(ids.Select(id => id.ToString()));

            try
            {
                var response = await _commentServiceClient.GetCommentCountsAsync(
                    request,
                    cancellationToken: cancellationToken
                );

                var counts = ids.ToDictionary(id => id, _ => 0);
                foreach (var item in response.Counts)
                {
                    if (Guid.TryParse(item.ReviewId, out var reviewId))
                    {
                        counts[reviewId] = item.Count;
                    }
                }

                return counts;
            }
            catch (RpcException ex)
            {
                _logger.LogWarning(ex, "Comment count aggregation failed with status {StatusCode}", ex.StatusCode);
                return ids.ToDictionary(id => id, _ => 0);
            }
        }

        private static long ToUnixMilliseconds(DateTime value)
        {
            return new DateTimeOffset(value.Kind == DateTimeKind.Unspecified
                ? DateTime.SpecifyKind(value, DateTimeKind.Utc)
                : value.ToUniversalTime()).ToUnixTimeMilliseconds();
        }
    }
}
