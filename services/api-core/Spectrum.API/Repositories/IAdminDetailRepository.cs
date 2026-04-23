using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Models;

namespace Spectrum.API.Repositories
{
    public interface IAdminDetailRepository
    {
        Task<AdminDetail> GetAdminDetailByUserIdAsync(Guid userId);
        Task AddAdminDetailAsync(AdminDetail adminDetail);
        Task<bool> UpdateAdminDetailAsync(AdminDetail adminDetail);
        Task<AdminDetail> GetAdminDetailByEmail(string email);
    }

    public class AdminDetailRepository : IAdminDetailRepository
    {
        private readonly SpectrumDbContext _context;
        public AdminDetailRepository(SpectrumDbContext context)
        {
            _context = context;
        }
        public async Task AddAdminDetailAsync(AdminDetail adminDetail)
        {
            _context.AdminDetails.Add(adminDetail);
            await _context.SaveChangesAsync();
        }
        public async Task<AdminDetail> GetAdminDetailByEmail(string email)
        {
            return await _context.AdminDetails
                .FirstOrDefaultAsync(ad => ad.User.Email == email) ?? new AdminDetail();
        }
        public async Task<AdminDetail> GetAdminDetailByUserIdAsync(Guid userId)
        {
            return await _context.AdminDetails
                .FirstOrDefaultAsync(ad => ad.UserId == userId) ?? new AdminDetail();
        }
        public async Task<bool> UpdateAdminDetailAsync(AdminDetail adminDetail)
        {
            throw new NotImplementedException();
        }
    }
}
