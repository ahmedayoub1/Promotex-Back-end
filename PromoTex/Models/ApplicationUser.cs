using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PromoTex.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? FullAddress { get; set; }

    }
}
