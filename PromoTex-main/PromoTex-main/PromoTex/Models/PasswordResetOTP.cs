using System.ComponentModel.DataAnnotations;

namespace PromoTex.Models
{
    public class PasswordResetOTP
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string OTP { get; set; }

        [Required]
        public DateTime ExpiryTime { get; set; }

        public bool IsUsed { get; set; } = false;
        public bool IsVerified { get; set; } = false;
    }
}
