namespace PromoTex.ModelViews
{
    public class ApplicationUserVM
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FullAddress { get; set; }
        public IEnumerable<string> Roles { get; set; } = new List<string>();
    }
}
