using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    public interface IReviewRepository
    {
        Task<Review?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Review>> GetByGameIdAsync(int gameId);
        Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId);
        Task<Review> AddAsync(Review review);
        Task SaveChangesAsync();
    }

    public class ReviewRepository : IReviewRepository
    {
        private readonly SpectrumDbContext _context;

        public ReviewRepository(SpectrumDbContext context)
        {
            _context = context;
        }

        public async Task<Review> AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);

            return review;
        }

        public async Task<IReadOnlyList<Review>> GetByGameIdAsync(int gameId)
        {
            return await _context.Reviews
                .Include(review => review.User)
                .Where(review => review.GameId == gameId)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetByIdAsync(Guid id)
        {
            return await _context.Reviews
                .Include(review => review.User)
                .FirstOrDefaultAsync(review => review.Id == id);
        }

        public async Task<IReadOnlyList<Review>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Reviews
                .Include(review => review.User)
                .Where(review => review.UserId == userId)
                .OrderByDescending(review => review.CreatedAt)
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
