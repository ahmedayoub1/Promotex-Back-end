using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PromoTex.Data_Access;
using PromoTex.DTO;
using PromoTex.Models;
using PromoTex.ModelViews;
using PromoTex.Services;
using PromoTex.Utility;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;

namespace PromoTex.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IConfiguration config;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly ApplicationDbContext applicationDb;
        private readonly ITemplateRenderer templateRenderer;
        private readonly IEmailSenderService emailSender;

        public AccountController(UserManager<ApplicationUser> userManager, 
            IConfiguration config,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext applicationDb,
             ITemplateRenderer templateRenderer,
             IEmailSenderService emailSender
            )
        {
            this.userManager = userManager;
            this.config = config;
            this.signInManager = signInManager;
            this.applicationDb = applicationDb;
            this.templateRenderer = templateRenderer;
            this.emailSender = emailSender;
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterDTO userDTO)
        {
            if (ModelState.IsValid)
            {
                ApplicationUser user = new ApplicationUser()
                {
                    Email = userDTO.Email,
                    UserName = userDTO.Email.Split('@')[0],
                    FullName = userDTO.FullName,
                    FullAddress = userDTO.FullAddress,
                    PhoneNumber = userDTO.PhoneNumber,
                };

                var result = await userManager.CreateAsync(user, userDTO.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, StaticData.CustomerRole);

                    
                    var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
                    var encodedToken = WebUtility.UrlEncode(token);

                   
                    var baseUrl = $"{Request.Scheme}://{Request.Host}";
                    var confirmUrl = $"{baseUrl}/api/user/confirm-email?userId={user.Id}&token={encodedToken}";

                   
                    var htmlContent = await templateRenderer.RenderTemplateAsync("Templates/EmailConfirmation.html",
                        new Dictionary<string, string>
                        {
                    { "name", user.UserName },
                    { "action_url", confirmUrl }
                        });

                   
                    await emailSender.SendEmailAsync(user.Email, "Confirm your email", htmlContent);

                    return Ok("User created! Please check your email to confirm your account.");
                }

                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError("", item.Description);
                }
            }

            return BadRequest(ModelState);
        }


        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginDTO)
        {
            var appUser = await userManager.FindByEmailAsync(loginDTO.Email);

            if (appUser != null)
            {
               
                if (!appUser.EmailConfirmed)
                {
                    ModelStateDictionary errors = new();
                    errors.AddModelError("Error", "Please confirm your email before logging in.");
                    return BadRequest(errors);
                }

               
                if (await userManager.IsLockedOutAsync(appUser))
                {
                    ModelStateDictionary errors = new();
                    errors.AddModelError("Error", "Your account is locked. Please try again later or contact support.");
                    return BadRequest(errors);
                }

                var result = await userManager.CheckPasswordAsync(appUser, loginDTO.Password);

                if (result)
                {
                    await signInManager.SignInAsync(appUser, loginDTO.RemeberMe);
                    return Ok("Login Succesfully");
                }
                else
                {
                    ModelStateDictionary errors = new();
                    errors.AddModelError("Error", "Invalid email or password.");
                    return BadRequest(errors);
                }
            }

            return NotFound();
        }


        [HttpPost("Logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return Ok("User is signed out");
        }
    }
}
