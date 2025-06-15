using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoTex.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public string FullName { get; set; }
        public string? FullAddress { get; set; }

        [NotMapped]
        public ICollection<Product> Products { get; set; }
        [NotMapped]
        public ICollection<Cart> Carts { get; set; }
        [NotMapped]
        public ICollection<Order> Orders { get; set; }
        [NotMapped]
        public ICollection<Store> Stores { get; set; }
    }
}
