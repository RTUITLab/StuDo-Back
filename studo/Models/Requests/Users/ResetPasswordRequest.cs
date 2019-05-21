using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Users
{
    public class ResetPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
