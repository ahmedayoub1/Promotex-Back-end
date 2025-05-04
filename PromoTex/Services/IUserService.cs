using Microsoft.AspNetCore.Identity;
using PromoTex.DTO;
using PromoTex.ModelViews;

namespace PromoTex.Services
{
    public interface IUserService
    {
        Task<List<ApplicationUserVM>> GetAllUsersWithRolesAsync();
        Task<bool> RegisterNewUserAsync(RegisterNewUserRequest request);
        Task<bool> ChangeUserRoleAsync(ChangeUserRoleRequest request);
        Task<string> ToggleUserLockAsync(string userId);
        Task<UserDetailsDTO> GetUserByIdAsync(string userId);
        Task<bool> UpdateUserAsync(string userId, UserDetailsDTO updatedUser);
        Task<bool> DeleteUserAsync(string userId);
        Task<IdentityResult> ConfirmEmailAsync(string userId, string token);
    }
}
