using System.Text.Json.Serialization;

namespace PromoTex.DTO
{
    public class LoginDTO
    {
        public string Email { get; set; }
        public string Password { get; set; }
        [JsonPropertyName("remeberMe")]
        public bool RemeberMe { get; set; } = true;
    }
}
