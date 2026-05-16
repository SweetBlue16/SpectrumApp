using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Dtos.Profile;
using Spectrum.API.Exceptions;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileService"/> class.
        /// </summary>
        /// <param name="userRepository">The repository used to access user data.</param>
        /// <param name="context">The database context used for relation lookups.</param>
        /// <param name="imageStorageService">The service used for direct image uploads to AWS S3.</param>
        public ProfileService(IUserRepository userRepository, SpectrumDbContext context, IImageStorageService imageStorageService)
        {
            _userRepository = userRepository;
            _context = context;
            _imageStorageService = imageStorageService;
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
            var user = await _userRepository.GetUserWithProfileDataAsync(userId);

            if (user == null)
            {
                throw new SpectrumNotFoundException("User not found.");
            }

            user.Biography = profileDto.Biography;
            user.ProfilePicture = profileDto.ProfilePicture;

            user.InterestedGames.Clear();
            var validGameGuids = profileDto.InterestedGames
                .Select(g => g.Id)
                .Where(id => Guid.TryParse(id, out _)) 
                .Select(Guid.Parse)
                .ToList();

            var selectedGames = await _context.Games
                .Where(g => validGameGuids.Contains(g.Id))
                .ToListAsync();

            foreach (var game in selectedGames)
            {
                user.InterestedGames.Add(game);
            }

            user.Platforms.Clear();
            var platformIds = profileDto.Platforms.Select(p => p.Id).ToList();
            var selectedPlatforms = await _context.Platforms
                .Where(p => platformIds.Contains(p.Id))
                .ToListAsync();

            foreach (var platform in selectedPlatforms)
            {
                user.Platforms.Add(platform);
            }

            await _userRepository.UpdateUserAsync(user);
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

            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return avatarUrl;
        }
    }
}