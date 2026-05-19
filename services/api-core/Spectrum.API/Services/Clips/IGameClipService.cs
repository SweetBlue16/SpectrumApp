using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Dtos.Media;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories; 
using Spectrum.API.Services.Storage;
using Spectrum.API.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Spectrum.API.Services.Clips
{
    /// <summary>
    /// Represents a summary of a game clip optimized for profile views.
    /// Maps perfectly with the frontend ClipData constraints.
    /// </summary>
    public class GameClipSummaryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? GameName { get; set; }
        public string Url { get; set; } = string.Empty; 
        public int LikesCount { get; set; } 
        public int DislikesCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// Defines the contract for managing game clips.
    /// Handles the business logic for creating, retrieving, and deleting clips in the database.
    /// </summary>
    public interface IGameClipService
    {
        /// <summary>
        /// Creates a new game clip record in the database after a successful upload to AWS S3.
        /// </summary>
        Task CreateClipAsync(Guid userId, CompleteUploadRequestDto request, string videoUrl);

        /// <summary>
        /// Retrieves all game clips belonging to a specific user.
        /// </summary>
        Task<IEnumerable<GameClipSummaryDto>> GetClipsByUserIdAsync(Guid userId);

        /// <summary>
        /// Deletes a specific game clip from the database and removes its associated physical file from AWS S3.
        /// </summary>
        Task DeleteClipAsync(Guid clipId, Guid userId);
    }

    /// <summary>
    /// Implementation of <see cref="IGameClipService"/> for managing game clips.
    /// </summary>
    public class GameClipService : IGameClipService
    {
        private readonly SpectrumDbContext dbContext;
        private readonly IVideoStorageService videoStorageService;
        private readonly IGameRepository gameRepository; 

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClipService"/> class.
        /// </summary>
        public GameClipService(SpectrumDbContext dbContext, IVideoStorageService videoStorageService, IGameRepository gameRepository)
        {
            this.dbContext = dbContext;
            this.videoStorageService = videoStorageService;
            this.gameRepository = gameRepository;
        }

        /// <inheritdoc />
        public async Task CreateClipAsync(Guid userId, CompleteUploadRequestDto request, string videoUrl)
        {
            var gameExists = await dbContext.Games.AnyAsync(g => g.Id == request.GameId);
            
            if (!gameExists)
            {
                // Si no existe, lo rescatamos del catálogo en memoria usando su Guid
                var gameFromCatalog = gameRepository.GetByGuid(request.GameId);
                
                if (gameFromCatalog == null)
                {
                    throw new SpectrumNotFoundException("The specified game was not found in the catalog cache.");
                }

                await dbContext.Games.AddAsync(gameFromCatalog);
                await dbContext.SaveChangesAsync();
            }

            var newClip = new GameClip
            {
                UserId = userId,
                GameId = request.GameId,
                Title = request.Title,
                Description = request.Description,
                Url = videoUrl,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.GameClips.Add(newClip);
            await dbContext.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<GameClipSummaryDto>> GetClipsByUserIdAsync(Guid userId)
        {
            var clips = await dbContext.GameClips
                .Include(c => c.Game)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            return clips.Select(c => new GameClipSummaryDto
            {
                Id = c.Id,
                Title = c.Title,
                ThumbnailUrl = null, 
                GameName = c.Game?.Title,
                Url = c.Url, 
                LikesCount = 0, 
                DislikesCount = 0,
                CreatedAt = c.CreatedAt
            });
        }

        /// <inheritdoc />
        public async Task DeleteClipAsync(Guid clipId, Guid userId)
        {
            var clip = await dbContext.GameClips.FirstOrDefaultAsync(c => c.Id == clipId);
            
            if (clip == null)
            {
                throw new SpectrumNotFoundException("The requested clip was not found.");
            }

            await ValidateDeletionPermissionAsync(clip, userId);

            await videoStorageService.DeleteVideoAsync(clip.Url);

            dbContext.GameClips.Remove(clip);
            await dbContext.SaveChangesAsync();
        }

        private async Task ValidateDeletionPermissionAsync(GameClip clip, Guid userId)
        {
            if (clip.UserId == userId)
            {
                return;
            }

            var requestingUser = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId);
            
            if (requestingUser?.Role != Constants.Roles.Admin)
            {
                throw new SpectrumForbiddenException("You do not have permission to delete this clip.");
            }
        }
    }
}