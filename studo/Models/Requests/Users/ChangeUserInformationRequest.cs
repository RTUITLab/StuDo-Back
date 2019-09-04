using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Users
{
    public class ChangeUserInformationRequest
    {
        [Required]
        public Guid Id { get; set; }
        public string StudentCardNumber { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
    }
}
