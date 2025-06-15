using PromoTex.Enums;
namespace PromoTex.Models
{
    public class RoleChangeRequest
    {
        public int Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string RequestedRole { get; set; }

        public RoleRequestStatus Status { get; set; } = RoleRequestStatus.Pending;

        public DateTime RequestDate { get; set; } = DateTime.UtcNow;

        public DateTime? DecisionDate { get; set; }
    }
}
