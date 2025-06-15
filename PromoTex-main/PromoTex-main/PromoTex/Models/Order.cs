using PromoTex.Enums;
using System.ComponentModel.DataAnnotations.Schema;

namespace PromoTex.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string BuyerId { get; set; }  // user placing the order
        public int StoreId { get; set; }  // seller

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public string ShippingAddress { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalPrice { get; set; }
        public OrderStatus Status { get; set; }
        //Buyer
        [NotMapped]
        public ApplicationUser Buyer { get; set; }
        public Store Store { get; set; }
        public ICollection<OrderItem> Items { get; set; }
    }
}
