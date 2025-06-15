using System.ComponentModel.DataAnnotations.Schema;

namespace PromoTex.Models
{
    public class ContactRequest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Message { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
        [NotMapped]
        public ApplicationUser User { get; set; }
    }
}
