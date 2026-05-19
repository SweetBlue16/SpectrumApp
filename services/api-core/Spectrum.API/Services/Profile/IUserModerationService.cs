using Spectrum.API.Dtos.Profile;
using Spectrum.API.Exceptions;
using Spectrum.API.Repositories;
using Spectrum.API.Utilities;

namespace Spectrum.API.Services.Profile
{
    public interface IUserModerationService
    {
        Task ToggleSuspensionAsync(Guid targetUserId, bool suspend, CancellationToken cancellationToken = default);
        Task<PagedResult<UserModerationDto>> GetUsersForModerationAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default);
        Task<AdminUserDetailDto> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }

    public class UserModerationService : IUserModerationService
    {
        private readonly IUserRepository _userRepository;

        public UserModerationService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<AdminUserDetailDto> GetUserDetailAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByIdAsync(userId);
            if (user == null || user.IsDeleted)
            {
                throw new SpectrumNotFoundException(Constants.ErrorMessages.UserNotFound);
            }

            var totalReviews = await _userRepository.GetTotalReviewsCountAsync(userId);
            var totalClips = await _userRepository.GetTotalClipsCountAsync(userId);

            return new AdminUserDetailDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                IsSuspended = user.IsSuspended,
                IsDeleted = user.IsDeleted,
                CreatedAt = user.CreatedAt,
                AvatarUrl = user.ProfilePicture,
                TotalReviews = totalReviews,
                TotalClips = totalClips
            };
        }

        public async Task<PagedResult<UserModerationDto>> GetUsersForModerationAsync(int page, int pageSize, string? searchTerm, CancellationToken cancellationToken = default)
        {
            var pagedUsers = await _userRepository.GetPaginatedUsersAsync(page, pageSize, searchTerm, cancellationToken);

            var dtos = pagedUsers.Items.Select(u => new UserModerationDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role,
                IsSuspended = u.IsSuspended,
                CreatedAt = u.CreatedAt
            }).ToList();
            
            return new PagedResult<UserModerationDto>
            {
                Items = dtos,
                TotalCount = pagedUsers.TotalCount,
                Page = pagedUsers.Page,
                PageSize = pagedUsers.PageSize
            };
        }

        public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var user = _userRepository.GetUserByIdAsync(userId).Result;
            if (user == null || user.IsSuspended)
            {
                throw new SpectrumNotFoundException(Constants.ErrorMessages.UserNotFound);
            }

            if (user.Role == Constants.Roles.Admin)
            {
                throw new SpectrumForbiddenException(Constants.ErrorMessages.AdminSanctionForbidden);
            }

            user.IsDeleted = true;
            user.IsSuspended = true;
            await _userRepository.UpdateUserAsync(user);
        }

        public async Task ToggleSuspensionAsync(Guid targetUserId, bool suspend, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetUserByIdAsync(targetUserId);

            if (user == null || user.IsDeleted)
            {
                throw new SpectrumNotFoundException(Constants.ErrorMessages.UserNotFound);
            }

            if (user.Role == Constants.Roles.Admin)
            {
                throw new SpectrumForbiddenException(Constants.ErrorMessages.AdminSanctionForbidden);
            }

            if (user.IsSuspended == suspend)
            {
                throw new SpectrumBusinessException(Constants.ErrorMessages.AccountAlreadySuspended);
            }

            user.IsSuspended = suspend;

            await _userRepository.UpdateUserAsync(user);
        }
    }
}
