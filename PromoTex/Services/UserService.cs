using Microsoft.AspNetCore.Identity;
using PromoTex.DTO;
using PromoTex.Models;
using PromoTex.ModelViews;

namespace PromoTex.Services
{
    public class UserService : IUserService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<bool> ChangeUserRoleAsync(ChangeUserRoleRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new Exception("User not found.");


            if (!await _roleManager.RoleExistsAsync(request.NewRole))
                throw new Exception($"Role '{request.NewRole}' does not exist.");


            var currentRoles = await _userManager.GetRolesAsync(user);
            if (currentRoles.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
                if (!removeResult.Succeeded)
                    throw new Exception("Failed to remove existing roles.");
            }


            var addResult = await _userManager.AddToRoleAsync(user, request.NewRole);
            if (!addResult.Succeeded)
                throw new Exception("Failed to assign new role.");

            return true;
        }


        public async Task<List<ApplicationUserVM>> GetAllUsersWithRolesAsync()
        {
            var users = _userManager.Users.ToList();

            var userVMs = new List<ApplicationUserVM>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userVMs.Add(new ApplicationUserVM
                {
                    UserId = user.Id,
                    UserName = user.UserName!,
                    FullName = user.FullName!,
                    Email = user.Email!,
                    FullAddress = user.FullAddress,
                    PhoneNumber = user.PhoneNumber!,
                    Roles = roles
                });
            }

            return userVMs;
        }

        public async Task<bool> RegisterNewUserAsync(RegisterNewUserRequest request)
        {
            if (!await _roleManager.RoleExistsAsync(request.Role))
                throw new Exception($"Role '{request.Role}' does not exist.");

            var user = new ApplicationUser
            {
                UserName = request.Email.Split('@')[0],
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                FullAddress = request.FullAddress,
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
                throw new Exception("Failed to create user: " + string.Join(", ", result.Errors.Select(e => e.Description)));

            var addRoleResult = await _userManager.AddToRoleAsync(user, request.Role);
            if (!addRoleResult.Succeeded)
                throw new Exception("User created but failed to assign role.");

            return true;
        }
        public async Task<string> ToggleUserLockAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var isLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow;

            if (isLocked)
            {
                var result = await _userManager.SetLockoutEndDateAsync(user, null);
                if (!result.Succeeded)
                    throw new Exception("Failed to unlock user.");
                return "User unlocked successfully.";
            }
            else
            {
                var result = await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                if (!result.Succeeded)
                    throw new Exception("Failed to lock user.");
                 return "User locked successfully.";
            }
        }
        public async Task<UserDetailsDTO> GetUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            return new UserDetailsDTO
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber
            };
        }

        public async Task<bool> UpdateUserAsync(string userId, UserDetailsDTO updatedUser)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            if (!string.IsNullOrWhiteSpace(updatedUser.UserName))
                user.UserName = updatedUser.UserName;
           

            if (!string.IsNullOrWhiteSpace(updatedUser.FullName))
                user.FullName = updatedUser.FullName;
           

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
                user.Email = updatedUser.Email;
            

            if (!string.IsNullOrWhiteSpace(updatedUser.PhoneNumber))
                user.PhoneNumber = updatedUser.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(updatedUser.FullAddress))
                user.FullAddress = updatedUser.FullAddress;

            var result = await _userManager.UpdateAsync(user);
            return result.Succeeded;
        }


        public async Task<bool> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new Exception("User not found.");

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return IdentityResult.Failed(new IdentityError { Description = "User not found." });

            return await _userManager.ConfirmEmailAsync(user, token);
        }

    }
}
