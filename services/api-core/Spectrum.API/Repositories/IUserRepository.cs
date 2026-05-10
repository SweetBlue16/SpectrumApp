using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    /// <summary>
    /// Defines the contract for data access operations related to the User entity.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Checks if an email address is already present in the database.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email exists, false otherwise.</returns>
        Task<bool> EmailExistsAsync(string email);

        /// <summary>
        /// Checks if a username is already present in the database.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns>True if the username exists, false otherwise.</returns>
        Task<bool> UsernameExistsAsync(string username);

        /// <summary>
        /// Retrieves a user record by their registered email address.
        /// </summary>
        /// <param name="email">The email address to search for.</param>
        /// <returns>The matching <see cref="User"/> entity, or null if no user is found.</returns>
        Task<User?> GetUserByEmailAsync(string email);

        /// <summary>
        /// Retrieves a user record by their unique system identifier.
        /// </summary>
        /// <param name="id">The unique identifier (GUID) of the user.</param>
        /// <returns>The matching <see cref="User"/> entity, or null if no user is found.</returns>
        Task<User?> GetUserByIdAsync(Guid id);

        /// <summary>
        /// Retrieves a user record including its related interested games and platforms.
        /// </summary>
        Task<User?> GetUserWithProfileDataAsync(Guid id);

        /// <summary>
        /// Updates an existing user record in the database.
        /// </summary>
        Task UpdateUserAsync(User user);

        /// <summary>
        /// Persists a newly created user record to the database.
        /// </summary>
        /// <param name="user">The user entity to save.</param>
        /// <returns>The saved <see cref="User"/> entity, including any database-generated fields.</returns>
        Task<User> AddUserAsync(User user);
    }

    /// <summary>
    /// Implementation of the <see cref="IUserRepository"/> using Entity Framework Core.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly SpectrumDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="UserRepository"/> class.
        /// </summary>
        /// <param name="context">The Entity Framework database context.</param>
        public UserRepository(SpectrumDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<User> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        /// <inheritdoc />
        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <inheritdoc />
        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <inheritdoc />
        public async Task<User?> GetUserWithProfileDataAsync(Guid id)
        {
            return await _context.Users
                .Include(u => u.InterestedGames)
                .Include(u => u.Platforms)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <inheritdoc />
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
