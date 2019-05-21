using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models
{
    public class Tag_Organization
    {
        public Guid TagId { get; set; }

        public Guid OrganizationId { get; set; }
    }
}
