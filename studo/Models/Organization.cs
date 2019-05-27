using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public string Description { get; set; }

        public string Avatar { get; set; }

        public List<Ad> Ads { get; set; }
    }
}
