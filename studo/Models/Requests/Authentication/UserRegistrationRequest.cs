using System;
using System.Collections.Generic;
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
    }
}
