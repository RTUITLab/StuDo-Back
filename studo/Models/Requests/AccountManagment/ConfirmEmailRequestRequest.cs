using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.AccountManagment
{
    public class ConfirmEmailRequestRequest
    {
        [Required]
        public Guid UserId { get; set; }
        [Required]
        public string Token { get; set; }
        [EmailAddress]
        public string NewEmail { get; set; }
    }
}
