using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.AccountManagment
{
    public class ResetPasswordRequestRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Token { get; set; }
        [Required]
        [Compare(nameof(NewPasswordConfirm), ErrorMessage = "Passwords doesn't match")]
        public string NewPassword { get; set; }
        [Required]
        public string NewPasswordConfirm { get; set; }
    }
}
