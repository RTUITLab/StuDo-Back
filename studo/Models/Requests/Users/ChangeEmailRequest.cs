using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Users
{
    public class ChangeEmailRequest
    {
        [Required]
        public Guid Id { get; set; }
        [Required]
        [EmailAddress]
        public string OldEmail { get; set; }
        [Required]
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
