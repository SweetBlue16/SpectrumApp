using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Dtos.Profile;
using Spectrum.API.Exceptions;
using Spectrum.API.Models;
using Spectrum.API.Repositories;
using Spectrum.API.Services.Storage;

namespace Spectrum.API.Services.Profile
{
    /// <summary>
    /// Defines the contract for profile-related operations, including retrieval, updates, and account security.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Retrieves the profile information of a user based on their unique identifier.
        /// </summary>
        /// <param name="userId">The unique identifier (GUID) of the user.</param>
        /// <returns>A <see cref="UserProfileDto"/> containing the user's profile data.</returns>
        Task<UserProfileDto> GetUserProfileAsync(Guid userId);

        /// <summary>
        /// Updates the profile information for an existing user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user to update.</param>
        /// <param name="profileDto">The updated profile data.</param>
        Task UpdateUserProfileAsync(Guid userId, UserProfileDto profileDto);

        /// <summary>
        /// Updates the user's password after verifying the current one.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="passwordDto">The data containing current and new passwords.</param>
        Task ChangePasswordAsync(Guid userId, ChangePasswordDto passwordDto);

        /// <summary>
        /// Uploads a new profile picture to AWS S3 and updates the user's avatar URL record.
        /// </summary>
        /// <param name="userId">The unique identifier of the authenticated user.</param>
        /// <param name="file">The image file payload.</param>
        /// <returns>The public URL of the newly uploaded profile picture.</returns>
        Task<string> UpdateAvatarAsync(Guid userId, IFormFile file);
    }

    /// <summary>
    /// Service implementation for managing user profiles within the Spectrum platform.
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly SpectrumDbContext _context;
        private readonly IImageStorageService _imageStorageService;
        private readonly IGameRepository _gameRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileService"/> class.
        /// </summary>
        /// <param name="userRepository">The repository used to access user data.</param>
        /// <param name="context">The database context used for relation lookups.</param>
        /// <param name="imageStorageService">The service used for direct image uploads to AWS S3.</param>
        public ProfileService(IUserRepository userRepository, SpectrumDbContext context, IImageStorageService imageStorageService, IGameRepository gameRepository)
        {
            _userRepository = userRepository;
            _context = context;
            _imageStorageService = imageStorageService;
            _gameRepository = gameRepository;
        }

        /// <inheritdoc />
        public async Task<UserProfileDto> GetUserProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetUserWithProfileDataAsync(userId);

            if (user == null)
            {
                throw new SpectrumNotFoundException("The requested user profile was not found.");
            }

            return new UserProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture,
                Biography = user.Biography,
                InterestedGames = user.InterestedGames.Select(g => new ProfileGameDto
                {
                    Id = g.Id.ToString(),
                    Name = g.Title,
                    ImageUrl = g.CoverImageUrl
                }).ToList(),
                Platforms = user.Platforms.Select(p => new ProfilePlatformDto
                {
                    Id = p.Id,
                    Name = p.Name
                }).ToList()
            };
        }

        /// <inheritdoc />
        public async Task UpdateUserProfileAsync(Guid userId, UserProfileDto profileDto)
        {
            var user = await _context.Users
                .Include(u => u.InterestedGames)
                .Include(u => u.Platforms)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
            {
                throw new SpectrumNotFoundException("User not found.");
            }

            user.Biography = profileDto.Biography;
            user.ProfilePicture = profileDto.ProfilePicture;

            await SyncInterestedGamesAsync(user, profileDto.InterestedGames);
            await SyncPlatformsAsync(user, profileDto.Platforms);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordDto passwordDto)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                throw new SpectrumNotFoundException("User not found.");
            }

            bool isCurrentPasswordValid = BCrypt.Net.BCrypt.Verify(passwordDto.CurrentPassword, user.PasswordHash);

            if (!isCurrentPasswordValid)
            {
                throw new SpectrumUnauthorizedException("The current password provided is incorrect.");
            }
            
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(passwordDto.NewPassword);

            await _userRepository.UpdateUserAsync(user);
        }

        /// <inheritdoc />
        public async Task<string> UpdateAvatarAsync(Guid userId, IFormFile file)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                throw new SpectrumNotFoundException("User not found.");
            }

            string avatarUrl = await _imageStorageService.UploadImageAsync(file, "photoProfiles");

            user.ProfilePicture = avatarUrl;

            await _context.SaveChangesAsync();

            return avatarUrl;
        }

        /// <summary>
        /// Synchronizes the many-to-many relationship between a User and their Interested Games
        /// by resolving game entities from the in-memory catalog repository and attaching them
        /// to the EF Core change tracker, avoiding a direct database lookup against the games table.
        /// </summary>
        /// <param name="user">The tracked User entity whose interested games will be synchronized.</param>
        /// <param name="incomingGames">The list of games sent from the client to set as the new interest list.</param>
        private async Task SyncInterestedGamesAsync(User user, List<ProfileGameDto> incomingGames)
        {
            var incomingGameIds = incomingGames
                .Select(g => g.Id)
                .Where(id => Guid.TryParse(id, out _))
                .Select(Guid.Parse)
                .ToList();

            var gamesToRemove = user.InterestedGames
                .Where(g => !incomingGameIds.Contains(g.Id))
                .ToList();

            foreach (var game in gamesToRemove)
                user.InterestedGames.Remove(game);

            var existingGameIds = user.InterestedGames.Select(g => g.Id).ToHashSet();
            var gamesToAddIds = incomingGameIds.Where(id => !existingGameIds.Contains(id)).ToList();

            foreach (var gameId in gamesToAddIds)
            {
                var gameFromCatalog = _gameRepository.GetByGuid(gameId);
                if (gameFromCatalog is null) continue;

                var trackedGame = _context.ChangeTracker.Entries<Game>()
                    .FirstOrDefault(e => e.Entity.Id == gameId)?.Entity;

                if (trackedGame is null)
                {
                    var existsInDb = await _context.Games.AnyAsync(g => g.Id == gameId);
                    if (!existsInDb)
                    {
                        await _context.Games.AddAsync(gameFromCatalog);
                        await _context.SaveChangesAsync();
                    }

                    _context.Attach(gameFromCatalog);
                }

                var gameToAdd = trackedGame ?? gameFromCatalog;
                user.InterestedGames.Add(gameToAdd);
            }
        }

        /// <summary>
        /// Synchronizes the many-to-many relationship between a User and their Platforms
        /// by calculating the exact additions and removals to preserve Change Tracker state.
        /// </summary>
        private async Task SyncPlatformsAsync(User user, List<ProfilePlatformDto> incomingPlatforms)
        {
            var incomingPlatformIds = incomingPlatforms.Select(p => p.Id).ToList();

            var platformsToRemove = user.Platforms
                .Where(p => !incomingPlatformIds.Contains(p.Id))
                .ToList();

            foreach (var platform in platformsToRemove)
            {
                user.Platforms.Remove(platform);
            }

            var existingPlatformIds = user.Platforms.Select(p => p.Id).ToList();
            var platformsToAddIds = incomingPlatformIds.Except(existingPlatformIds).ToList();

            if (platformsToAddIds.Any())
            {
                var platformsToAdd = await _context.Platforms
                    .Where(p => platformsToAddIds.Contains(p.Id))
                    .ToListAsync();

                foreach (var platform in platformsToAdd)
                {
                    user.Platforms.Add(platform);
                }
            }
        }

    }
}