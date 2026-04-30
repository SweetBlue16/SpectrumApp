using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    /// <summary>
    /// Defines the contract for data access operations related to the administrator's extended personal details.
    /// </summary>
    public interface IAdminDetailRepository
    {
        /// <summary>
        /// Retrieves the extended details of an administrator using their associated user identifier.
        /// </summary>
        /// <param name="userId">The unique identifier (GUID) of the base user account.</param>
        /// <returns>The matching <see cref="AdminDetail"/> entity, or null if no details are found.</returns>
        Task<AdminDetail?> GetAdminDetailByUserIdAsync(Guid userId);

        /// <summary>
        /// Persists a new administrator detail record to the database.
        /// </summary>
        /// <param name="adminDetail">The administrator detail entity to save.</param>
        /// <returns>A task representing the asynchronous database insert operation.</returns>
        Task AddAdminDetailAsync(AdminDetail adminDetail);

        /// <summary>
        /// Updates an existing administrator detail record in the database.
        /// </summary>
        /// <param name="adminDetail">The administrator detail entity with the updated values.</param>
        /// <returns>True if the update was successful, false otherwise.</returns>
        /// <exception cref="NotImplementedException">Thrown because the update logic has not been implemented yet.</exception>
        Task<bool> UpdateAdminDetailAsync(AdminDetail adminDetail);

        /// <summary>
        /// Retrieves the extended details of an administrator using their associated user email address.
        /// </summary>
        /// <param name="email">The email address linked to the administrator's base user account.</param>
        /// <returns>The matching <see cref="AdminDetail"/> entity, or null if no details are found.</returns>
        Task<AdminDetail?> GetAdminDetailByEmail(string email);
    }

    /// <summary>
    /// Implementation of the <see cref="IAdminDetailRepository"/> using Entity Framework Core.
    /// </summary>
    public class AdminDetailRepository : IAdminDetailRepository
    {
        private readonly SpectrumDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AdminDetailRepository"/> class.
        /// </summary>
        /// <param name="context">The Entity Framework database context.</param>
        public AdminDetailRepository(SpectrumDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task AddAdminDetailAsync(AdminDetail adminDetail)
        {
            _context.AdminDetails.Add(adminDetail);
            await _context.SaveChangesAsync();
        }

        /// <inheritdoc />
        public async Task<AdminDetail?> GetAdminDetailByEmail(string email)
        {
            return await _context.AdminDetails.FirstOrDefaultAsync(ad => ad.User.Email == email);
        }

        /// <inheritdoc />
        public async Task<AdminDetail?> GetAdminDetailByUserIdAsync(Guid userId)
        {
            return await _context.AdminDetails.FirstOrDefaultAsync(ad => ad.UserId == userId);
        }

        /// <inheritdoc />
        public async Task<bool> UpdateAdminDetailAsync(AdminDetail adminDetail)
        {
            throw new NotImplementedException();
        }
    }
}
