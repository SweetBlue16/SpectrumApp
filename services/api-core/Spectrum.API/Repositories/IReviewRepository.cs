using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Review>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Review> AddAsync(Review review, CancellationToken cancellationToken = default);
        Task UpdateCountersAsync(Guid reviewId, int likesCount, int dislikesCount, CancellationToken cancellationToken = default);
        Task SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<Dictionary<int, double>> GetAverageRatingsAsync(CancellationToken cancellationToken = default);
    }

    public class ReviewRepository : IReviewRepository
    {
        private readonly SpectrumDbContext _context;

        public ReviewRepository(SpectrumDbContext context)
        {
            _context = context;
        }

        public async Task<Review> AddAsync(Review review, CancellationToken cancellationToken = default)
        {
            await _context.Reviews.AddAsync(review, cancellationToken);

            return review;
        }

        public async Task<IReadOnlyList<Review>> GetByGameIdAsync(int gameId, CancellationToken cancellationToken = default)
        {
            return await _context.Reviews
                .Include(review => review.User)
                .Where(review => review.GameId == gameId)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Review?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Reviews
                .Include(review => review.User)
                .FirstOrDefaultAsync(review => review.Id == id, cancellationToken);
        }

        public async Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.Reviews
                .Include(review => review.User)
                .Where(review => review.UserId == userId)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task UpdateCountersAsync(
            Guid reviewId,
            int likesCount,
            int dislikesCount,
            CancellationToken cancellationToken = default
        )
        {
            var review = await _context.Reviews.FirstOrDefaultAsync(
                review => review.Id == reviewId,
                cancellationToken
            );

            if (review is null)
            {
                return;
            }

            review.LikesCount = likesCount;
            review.DislikesCount = dislikesCount;
            review.UpdatedAt = DateTime.UtcNow;
        }

        public async Task<Dictionary<int, double>> GetAverageRatingsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Reviews
                .GroupBy(r => r.GameId) 
                .Select(group => new
                {
                    GameId = group.Key,
                    Average = group.Average(r => r.Rating) 
                })
                .ToDictionaryAsync(
                    x => x.GameId,
                    x => x.Average,
                    cancellationToken
                );
        }

        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
