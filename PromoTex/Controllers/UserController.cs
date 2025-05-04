using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PromoTex.Data_Access;
using PromoTex.DTO;
using PromoTex.Models;
using PromoTex.ModelViews;
using PromoTex.Services;
using System.Net;
using System.Text;
using System.Web;


namespace PromoTex.Controllers
{
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



    }
}
