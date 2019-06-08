using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace studo.Models
{
    public class User : IdentityUser<Guid>
    {
        public string StudentCardNumber { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }

        public List<Ad> Ads { get; set; }

        public List<Resume> Resumes { get; set; }
    }
}
