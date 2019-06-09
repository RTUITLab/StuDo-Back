using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models.Requests.Users
{
    public class ChangePasswordRequest
    {
        [Required]
        [EmailAddress]
        // User email
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
