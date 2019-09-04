using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Users
{
    public class ChangePasswordRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
