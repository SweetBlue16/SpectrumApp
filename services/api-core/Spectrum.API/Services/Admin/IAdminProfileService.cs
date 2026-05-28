using Microsoft.EntityFrameworkCore;
using Spectrum.API.Data;
using Spectrum.API.Dtos.Admin;
using Spectrum.API.Exceptions;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Admin
{
    public interface IAdminProfileService
    {
        Task<AdminProfileDto> GetProfileAsync(Guid adminId, CancellationToken cancellationToken = default);
        Task<AdminProfileDto> UpdateProfileAsync(Guid adminId, UpdateAdminProfileDto dto, CancellationToken cancellationToken = default);
    }

    public class AdminProfileService : IAdminProfileService
    {
        private readonly SpectrumDbContext _context;

        public AdminProfileService(SpectrumDbContext context)
        {
            _context = context;
        }

        public async Task<AdminProfileDto> GetProfileAsync(Guid adminId, CancellationToken cancellationToken = default)
        {
            var user = await LoadAdminAsync(adminId, cancellationToken);
            return MapProfile(user);
        }

        public async Task<AdminProfileDto> UpdateProfileAsync(Guid adminId, UpdateAdminProfileDto dto, CancellationToken cancellationToken = default)
        {
            var user = await LoadAdminAsync(adminId, cancellationToken);
            var normalizedUsername = dto.Username.Trim();

            var usernameInUse = await _context.Users
                .AnyAsync(candidate => candidate.Id != adminId && candidate.Username == normalizedUsername, cancellationToken);
            if (usernameInUse)
            {
                throw new SpectrumBusinessException(Constants.ErrorMessages.UsernameAlreadyTaken);
            }

            user.Username = normalizedUsername;
            user.ProfilePicture = string.IsNullOrWhiteSpace(dto.ProfilePicture) ? user.ProfilePicture : dto.ProfilePicture.Trim();
            user.AdminDetail!.FirstName = dto.FirstName.Trim();
            user.AdminDetail.LastName = dto.LastName.Trim();
            user.AdminDetail.PhoneNumber = dto.PhoneNumber.Trim();
            user.AdminDetail.Address = dto.Address.Trim();

            await _context.SaveChangesAsync(cancellationToken);
            return MapProfile(user);
        }

        private async Task<Models.User> LoadAdminAsync(Guid adminId, CancellationToken cancellationToken)
        {
            var user = await _context.Users
                .Include(item => item.AdminDetail)
                .FirstOrDefaultAsync(item =>
                    item.Id == adminId &&
                    item.Role == Constants.Roles.Admin &&
                    !item.IsDeleted,
                    cancellationToken);

            if (user?.AdminDetail is null)
            {
                throw new SpectrumNotFoundException(Constants.ErrorMessages.ResourceNotFound);
            }

            return user;
        }

        private static AdminProfileDto MapProfile(Models.User user)
        {
            var detail = user.AdminDetail!;
            return new AdminProfileDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = detail.FirstName,
                LastName = detail.LastName,
                PhoneNumber = detail.PhoneNumber,
                Address = detail.Address,
                Rfc = detail.Rfc,
                ProfilePicture = user.ProfilePicture,
                Role = user.Role
            };
        }
    }
}
