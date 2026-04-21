using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    public interface IUserRepository
    {
        Task<bool> EmailExistsAsync(string email);
        Task<bool> UsernameExistsAsync(string username);
        Task<User> GetUserByEmailAsync(string email);
        Task AddUserAsync(User user);
    }

    public class UserRepository : IUserRepository
    {
        private readonly SpectrumDbContext _context;

        public UserRepository(SpectrumDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists a new user record to the database.
        /// </summary>
        /// <param name="user">The user entity to save.</param>
        public async Task AddUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks if an email address is already present in the database.
        /// </summary>
        /// <param name="email">The email to validate.</param>
        /// <returns>True if the email exists, false otherwise.</returns>
        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Users.AnyAsync(u => u.Email == email);
        }

        /// <summary>
        /// Looks up a user record by their registered email address.
        /// </summary>
        /// <param name="email">The email to search for.</param>
        /// <returns>The matching user or null.</returns>
        public async Task<User> GetUserByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        /// <summary>
        /// Checks if a username is already present in the database.
        /// </summary>
        /// <param name="username">The username to validate.</param>
        /// <returns>True if the username exists, false otherwise.</returns>
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.Users.AnyAsync(u => u.Username == username);
        }
    }
}
