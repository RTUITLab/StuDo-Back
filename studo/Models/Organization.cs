using System;
using System.Collections.Generic;

namespace studo.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Ad> Ads { get; set; }

        public List<UserRightsInOrganization> Users { get; set; }
    }
}
