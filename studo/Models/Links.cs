using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models
{
    public class Links
    {
        public Guid Id { get; set; }

        public LinkType LinkType { get; set; }

        public string Url { get; set; }
    }
}
