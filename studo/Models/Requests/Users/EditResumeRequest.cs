using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Users
{
    public class EditResumeRequest
    {
        [Required]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
