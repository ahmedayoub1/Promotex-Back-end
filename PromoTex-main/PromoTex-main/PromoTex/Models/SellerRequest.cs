using System.ComponentModel.DataAnnotations;

namespace PromoTex.Models
{
    public class SellerRequests
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string userId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }

        [Required(ErrorMessage = "Store Name is required.")]
        [StringLength(150, ErrorMessage = "Store Name cannot exceed 150 characters.")]
        public string StoreName { get; set; }
        public string? FacebookLink { get; set; }
        public string? InstgramLink { get; set; }
        public string? WebsiteLink { get; set; }
        [Required]
        public string ProofOfAddressUrl { get; set; }
        [Required]
        public string ShippingAddress { get; set; }
        public string? StoreRelatedPhotoUrl { get; set; }
        [Required]
        public string EmergencyPhoneNumber { get; set; }
        [Required]
        public string AdditionalPhoneNumber { get; set; }
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ApprovedAt { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
    }

    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
}
