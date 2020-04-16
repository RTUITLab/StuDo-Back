using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Authentication
{
    public class RefreshTokenRequest
    {
        [Required]
        public string RefreshToken { get; set; }
    }
}
