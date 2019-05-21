using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Query.ResultOperators.Internal;

namespace studo.Models
{
    // Store images for
    // instagram, vk, facebook, twitter, github
    public class LinkType
    {
        public Guid Id { get; set; }

        public string Image { get; set; }

        public string Name { get; set; }
    }
}
