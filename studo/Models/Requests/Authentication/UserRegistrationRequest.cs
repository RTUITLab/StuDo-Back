using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Authentication
{
    public class UserRegistrationRequest
    {
        [Required]
        public string Firstname { get; set; }
        [Required]
        public string Surname { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        
        public string StudentCardNumber { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare(nameof(Password), ErrorMessage = "Passwords doensn't match")]
        public string PasswordConfirm { get; set; }
    }
}
