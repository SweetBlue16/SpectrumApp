using Spectrum.API.Dtos.Profile;
using Spectrum.API.Repositories;

namespace Spectrum.API.Services.Profile
{
    /// <summary>
    /// Defines the contract for profile-related operations, such as retrieving user information.
    /// </summary>
    public interface IProfileService
    {
        /// <summary>
        /// Retrieves the profile information of a user based on their email address.
        /// </summary>
        /// <param name="email">The email of the user whose profile is being requested.</param>
        /// <returns>A <see cref="UserProfileDto"/> containing the user's profile data, or null if the user is not found.</returns>
        Task<UserProfileDto> GetUserProfileByEmailAsync(string email);
    }

    /// <summary>
    /// Service implementation for managing user profiles within the Spectrum platform.
    /// </summary>
    public class ProfileService : IProfileService
    {
        private readonly IUserRepository userRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProfileService"/> class.
        /// </summary>
        /// <param name="userRepository">The repository used to access user data.</param>
        public ProfileService(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }

        /// <inheritdoc />
        public async Task<UserProfileDto> GetUserProfileByEmailAsync(string email)
        {
            var user = await userRepository.GetUserByEmailAsync(email);

            if (user == null)
                return null;

            return new UserProfileDto
            {
                Username = user.Username,
                Email = user.Email,
                ProfilePicture = user.ProfilePicture
            };
        }
    }
}