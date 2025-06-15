using System.ComponentModel.DataAnnotations.Schema;

namespace PromoTex.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        [NotMapped]
        public ApplicationUser User { get; set; }

        public ICollection<CartItem> CartItems { get; set; }
    }
}
