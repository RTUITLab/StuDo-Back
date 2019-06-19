using System;
using System.Collections.Generic;

namespace studo.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Avatar { get; set; }

        public List<Ad> Ads { get; set; }

        public List<User_Organization> Users { get; set; }
    }
}
