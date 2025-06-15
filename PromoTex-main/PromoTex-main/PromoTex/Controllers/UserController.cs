using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.DTO;
using PromoTex.Enums;
using PromoTex.Models;
using PromoTex.ModelViews;
using PromoTex.Services;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Web;


namespace PromoTex.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IEmailSenderService _emailService;
        private readonly ITemplateRenderer _templateRenderer;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _dbContext;

        public UserManager<ApplicationUser> UserManager { get; }

        public UserController(
            IUserService userService,
            UserManager<ApplicationUser> userManager,
            IEmailSenderService emailService,
             ITemplateRenderer templateRenderer,
            IWebHostEnvironment env,
            ApplicationDbContext dbContext
            
            )
        {
            _userService = userService;
            UserManager = userManager;
            _emailService = emailService;
            _templateRenderer = templateRenderer;
            _env = env;
            _dbContext = dbContext;
        }

        [HttpGet("all-users-with-roles")]
        public async Task<IActionResult> GetAllUsersWithRoles()
        {
            var users = await _userService.GetAllUsersWithRolesAsync();
            return Ok(users);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            var user = await UserManager.FindByIdAsync(userId);
            if (user == null) return BadRequest("Invalid user");

            var result = await UserManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? Ok("Email confirmed successfully!") : BadRequest("Invalid or expired token.");
        }
        [HttpPost("resendemailconfirmation")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationDto model)
        {
            
            var user = await UserManager.FindByEmailAsync(model.Email);

            
            if (user == null || user.EmailConfirmed)
            {
                
                return Ok(new { Message = "If the account exists and has not been confirmed, a confirmation email has been sent." });
            }

            
            var token = await UserManager.GenerateEmailConfirmationTokenAsync(user);

          
            var encodedToken = System.Web.HttpUtility.UrlEncode(token);
            var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/user/confirm-email?userId={user.Id}&token={encodedToken}";


            var templateData = new Dictionary<string, string>
                {
                    { "name", user.UserName! },
                    { "action_url", confirmationLink }
                };

            
            var emailBody = await _templateRenderer.RenderTemplateAsync("Templates/EmailConfirmation.html", templateData);

           
            await _emailService.SendEmailAsync(user.Email!, "Confirm Your Email Address", emailBody);

           
            return Ok(new { Message = "If the account exists and has not been confirmed, a confirmation email has been sent." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
           
            var user = await UserManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest("User not found");

           
            var otp = new Random().Next(100000, 999999).ToString();

           
            var otpEntity = new PasswordResetOTP
            {
                UserId = user.Id,
                OTP = otp,
                ExpiryTime = DateTime.UtcNow.AddMinutes(10)
            };

            _dbContext.passwordResetOTPs.Add(otpEntity);
            await _dbContext.SaveChangesAsync();

           
            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "PasswordReset.html");
            string emailBody;

          
            if (System.IO.File.Exists(templatePath))
            {
                emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
                emailBody = emailBody.Replace("{{OTP}}", otp);
            }
            else
            {
                return StatusCode(500, "Error: Password reset email template not found.");
            }

            
            await _emailService.SendEmailAsync(model.Email, "PromoTex Password Reset Code", emailBody);

           
            return Ok("OTP sent to your email.");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OTPVerficationDTO model)
        {
          
            var otpRecord = await _dbContext.passwordResetOTPs
                .Where(x => x.OTP == model.OTP && !x.IsUsed)
                .OrderByDescending(x => x.ExpiryTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null || otpRecord.ExpiryTime < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP");

          
            otpRecord.IsVerified = true;
            await _dbContext.SaveChangesAsync();

            return Ok("OTP is valid. You may now reset your password.");
        }
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            
            var otpRecord = await _dbContext.passwordResetOTPs
                .Where(x => x.OTP == model.OTP && x.IsVerified && !x.IsUsed)
                .OrderByDescending(x => x.ExpiryTime)
                .FirstOrDefaultAsync();

            if (otpRecord == null || otpRecord.ExpiryTime < DateTime.UtcNow)
                return BadRequest("Invalid or expired OTP");

            var user = await UserManager.FindByIdAsync(otpRecord.UserId);
            if (user == null)
                return BadRequest("User not found");

            
            var token = await UserManager.GeneratePasswordResetTokenAsync(user);

            
            var result = await UserManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

           
            otpRecord.IsUsed = true;
            await _dbContext.SaveChangesAsync();

            return Ok("Password has been reset successfully.");
        }
        
        [HttpGet("User-Profile")]
        public async Task<IActionResult> GetUserDetails()
        {
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            return Ok(new
            {
                user.UserName,
                user.Email,
                user.FullName,
                user.FullAddress,
                user.PhoneNumber
            });
        }
        [HttpPost("toggle-lock/{userId}")]
        public async Task<IActionResult> ToggleUserLock(string userId)
        {
            try
            {
                var message = await _userService.ToggleUserLockAsync(userId);
                return Ok(new { message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("Delete-User/{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(userId);
                return result
                    ? Ok(new { message = "User deleted successfully." })
                    : BadRequest(new { message = "Failed to delete user." });
            }
            catch (Exception ex)
            {
                return NotFound(new { error = ex.Message });
            }
        }
        [HttpPut("Update-user-details")]
        public async Task<IActionResult> UpdateUserDetails([FromBody] UpdateUserDto dto)
        {
            var user = await UserManager.GetUserAsync(User);
            if (user == null)
                return NotFound("User not found.");

            if (!string.IsNullOrWhiteSpace(dto.FullName))
                user.FullName = dto.FullName;

            if (!string.IsNullOrWhiteSpace(dto.FullAddress))
                user.FullAddress = dto.FullAddress;

            if (!string.IsNullOrWhiteSpace(dto.PhoneNumber))
                user.PhoneNumber = dto.PhoneNumber;

            var result = await UserManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("User updated successfully.");
        }


        [Authorize]
        [HttpPost("request-role-change")]
        public async Task<IActionResult> RequestRoleChange([FromBody] string requestedRole)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Prevent duplicate pending requests
            bool alreadyRequested = await _dbContext.RoleChangeRequests
                .AnyAsync(r => r.UserId == userId && r.Status == RoleRequestStatus.Pending);

            if (alreadyRequested)
                return BadRequest("You already have a pending request.");

            var request = new RoleChangeRequest
            {
                UserId = userId,
                RequestedRole = requestedRole
            };

            _dbContext.RoleChangeRequests.Add(request);
            await _dbContext.SaveChangesAsync();

            return Ok("Your role change request has been submitted.");
        }


        [Authorize(Roles = "Admin")]
        [HttpGet("pending-role-requests")]
        public async Task<IActionResult> GetPendingRequests()
        {
            var requests = await _dbContext.RoleChangeRequests
                .Where(r => r.Status == RoleRequestStatus.Pending)
                .Include(r => r.User)
                .ToListAsync();

            return Ok(requests.Select(r => new
            {
                r.Id,
                r.UserId,
                UserName = r.User.UserName,
                r.RequestedRole,
                r.RequestDate
            }));
        }


        [Authorize(Roles = "Admin")]
        [HttpPost("handle-role-request")]
        public async Task<IActionResult> HandleRequest(int requestId, bool approve)
        {
            var request = await _dbContext.RoleChangeRequests
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request == null || request.Status != RoleRequestStatus.Pending)
                return NotFound("Request not found or already handled.");

            var user = request.User;

            // Update status
            request.Status = approve ? RoleRequestStatus.Approved : RoleRequestStatus.Rejected;
            request.DecisionDate = DateTime.UtcNow;

            if (approve)
            {
                // Remove current roles
                var currentRoles = await UserManager.GetRolesAsync(user);
                if (currentRoles.Any())
                {
                    var removeResult = await UserManager.RemoveFromRolesAsync(user, currentRoles);
                    if (!removeResult.Succeeded)
                        return BadRequest("Failed to remove current roles.");
                }

                // Add new role
                var addResult = await UserManager.AddToRoleAsync(user, request.RequestedRole);
                if (!addResult.Succeeded)
                    return BadRequest("Failed to add new role.");
            }

            await _dbContext.SaveChangesAsync();

            // Prepare email content
            var templateData = new Dictionary<string, string>
            {
                { "UserName", user.UserName },
                { "Result", approve ? "approved" : "rejected" },
                { "RequestedRole", request.RequestedRole }
            };

            var emailBody = await _templateRenderer.RenderTemplateAsync("Templates/RoleChangeNotification.html", templateData);

            // Send email to user
            await _emailService.SendEmailAsync(user.Email, "Role Change Update", emailBody);

            return Ok($"Request {(approve ? "approved" : "rejected")} successfully.");
        }

    }
}
