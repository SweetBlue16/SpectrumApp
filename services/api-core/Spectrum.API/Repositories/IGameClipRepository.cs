using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    /// <summary>
    /// Defines the contract for data access operations related to the <see cref="GameClip"/> entity.
    /// Encapsulates all direct interactions with the persistence layer to enforce separation of concerns.
    /// </summary>
    public interface IGameClipRepository
    {
        /// <summary>
        /// Checks if a game record exists in the local database by its unique identifier.
        /// </summary>
        /// <param name="gameId">The unique identifier (GUID) of the game.</param>
        /// <returns>True if the game exists in the database; otherwise, false.</returns>
        Task<bool> GameExistsAsync(Guid gameId);

        /// <summary>
        /// Prepares a new game entity to be persisted in the database.
        /// Used to mirror catalog metadata before associating it with media items.
        /// </summary>
        /// <param name="game">The game entity instance to add.</param>
        Task AddGameAsync(Game game);

        /// <summary>
        /// Prepares a newly created game clip record to be inserted into the database.
        /// </summary>
        Task AddClipAsync(GameClip clip);

        /// <summary>
        /// Retrieves all game clips belonging to a specific user, ordered descending by creation date.
        /// Includes the related game details in the query representation.
        /// </summary>
        /// <returns>A collection of matching <see cref="GameClip"/> entities.</returns>
        Task<IEnumerable<GameClip>> GetClipsByUserIdAsync(Guid userId);

        /// <summary>
        /// Retrieves a specific game clip record by its system unique identifier.
        /// </summary>
        /// <returns>The matching game clip entity, or null if no record matches the given ID.</returns>
        Task<GameClip?> GetClipByIdAsync(Guid clipId);

        /// <summary>
        /// Marks an existing game clip record as deleted without physically removing it.
        /// </summary>
        Task DeleteClipAsync(GameClip clip, Guid deletedByUserId);

        /// <summary>
        /// Persists all pending tracking changes asynchronously into the database backend.
        /// </summary>
        Task SaveChangesAsync();
    }

    /// <summary>
    /// Entity Framework Core implementation of the <see cref="IGameClipRepository"/> contract.
    /// Manages direct database persistence and state tracking for games and clips.
    /// </summary>
    public class GameClipRepository : IGameClipRepository
    {
        private readonly SpectrumDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClipRepository"/> class.
        /// </summary>
        /// <param name="context">The database context used for entity operations.</param>
        public GameClipRepository(SpectrumDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<bool> GameExistsAsync(Guid gameId)
        {
            return await _context.Games.AnyAsync(g => g.Id == gameId);
        }

        /// <inheritdoc />
        public async Task AddGameAsync(Game game)
        {
            await _context.Games.AddAsync(game);
        }

        /// <inheritdoc />
        public async Task AddClipAsync(GameClip clip)
        {
            await _context.GameClips.AddAsync(clip);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<GameClip>> GetClipsByUserIdAsync(Guid userId)
        {
            return await _context.GameClips
                .Include(c => c.Game)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        /// <inheritdoc />
        public async Task<GameClip?> GetClipByIdAsync(Guid clipId)
        {
            return await _context.GameClips.FirstOrDefaultAsync(c => c.Id == clipId);
        }

        /// <inheritdoc />
        public async Task DeleteClipAsync(GameClip clip, Guid deletedByUserId)
        {
            clip.IsDeleted = true;
            clip.DeletedAt = DateTime.UtcNow;
            clip.DeletedByUserId = deletedByUserId;
            await Task.CompletedTask;
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
