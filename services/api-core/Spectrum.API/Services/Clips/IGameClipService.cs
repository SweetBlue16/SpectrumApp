using Spectrum.API.Dtos.Media;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories;
using Spectrum.API.Services.Storage;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Clips
{
    /// <summary>
    /// Defines the contract for managing game clips.
    /// Handles the business logic for creating, retrieving, and deleting clips in the database.
    /// </summary>
    public interface IGameClipService
    {
        /// <summary>
        /// Creates a new game clip record in the database after a successful upload to AWS S3.
        /// </summary>
        /// <param name="request">The request payload containing metadata about the video upload.</param>
        /// <param name="videoUrl">The public cloud URL where the raw video file is stored.</param>
        Task CreateClipAsync(Guid userId, CompleteUploadRequestDto request, string videoUrl);

        /// <summary>
        /// Retrieves all game clips belonging to a specific user.
        /// </summary>
        /// <returns>A collection of <see cref="GameClipSummaryDto"/> optimized for view representations.</returns>
        Task<IEnumerable<GameClipSummaryDto>> GetClipsByUserIdAsync(Guid userId);

        /// <summary>
        /// Deletes a specific game clip from the database and removes its associated physical file from AWS S3.
        /// </summary>
        Task DeleteClipAsync(Guid clipId, Guid userId);
    }

    /// <summary>
    /// Implementation of <see cref="IGameClipService"/> for managing game clips.
    /// Operates completely decoupled from the infrastructure layer through repositories.
    /// </summary>
    public class GameClipService : IGameClipService
    {
        private readonly IGameClipRepository _clipRepository;
        private readonly IUserRepository _userRepository;
        private readonly IVideoStorageService _videoStorageService;
        private readonly IGameRepository _gameRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="GameClipService"/> class.
        /// </summary>
        /// <param name="clipRepository">The repository abstracted for data access operations related to game clips.</param>
        /// <param name="userRepository">The repository abstracted for user records and authorization lookups.</param>
        /// <param name="videoStorageService">The core service handling file lifecycles in cloud object storage.</param>
        /// <param name="gameRepository">The central memory repository acting as the snapshot catalog cache.</param>
        public GameClipService(
            IGameClipRepository clipRepository,
            IUserRepository userRepository,
            IVideoStorageService videoStorageService,
            IGameRepository gameRepository)
        {
            _clipRepository = clipRepository;
            _userRepository = userRepository;
            _videoStorageService = videoStorageService;
            _gameRepository = gameRepository;
        }

        /// <inheritdoc />
        public async Task CreateClipAsync(Guid userId, CompleteUploadRequestDto request, string videoUrl)
        {
            var gameExists = await _clipRepository.GameExistsAsync(request.GameId);

            if (!gameExists)
            {
                var gameFromCatalog = _gameRepository.GetByGuid(request.GameId);

                if (gameFromCatalog == null)
                {
                    throw new SpectrumNotFoundException("The specified game was not found in the catalog cache.");
                }

                await _clipRepository.AddGameAsync(gameFromCatalog);
                await _clipRepository.SaveChangesAsync();
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

            await _clipRepository.AddClipAsync(newClip);
            await _clipRepository.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<GameClipSummaryDto>> GetClipsByUserIdAsync(Guid userId)
        {
            var clips = await _clipRepository.GetClipsByUserIdAsync(userId);

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
            var clip = await _clipRepository.GetClipByIdAsync(clipId);

            if (clip == null)
            {
                throw new SpectrumNotFoundException("The requested clip was not found.");
            }

            await ValidateDeletionPermissionAsync(clip, userId);

            await _videoStorageService.DeleteVideoAsync(clip.Url);

            await _clipRepository.DeleteClipAsync(clip);
            await _clipRepository.SaveChangesAsync();
        }

        /// <summary>
        /// Evaluates identity and security constraints to determine if a removal operation can take place.
        /// </summary>
        private async Task ValidateDeletionPermissionAsync(GameClip clip, Guid userId)
        {
            if (clip.UserId == userId)
            {
                return;
            }

            var requestingUser = await _userRepository.GetUserByIdAsync(userId);

            if (requestingUser?.Role != Constants.Roles.Admin)
            {
                throw new SpectrumForbiddenException("You do not have permission to delete this clip.");
            }
        }
    }
}