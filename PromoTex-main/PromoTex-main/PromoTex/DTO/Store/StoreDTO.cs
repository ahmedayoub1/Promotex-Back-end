using System.ComponentModel.DataAnnotations;

namespace PromoTex.DTO.Store
{
    public class StoreDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Phone]
        [StringLength(20)]
        public string PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Address { get; set; }

        public IFormFile? LogoUrl { get; set; }
    }
}
