using System;
using System.Collections.Generic;

namespace studo.Models
{
    public class OrganizationRight
    {
        public Guid Id { get; set; }
        public string RightName { get; set; }
        public List<UserRightsInOrganiaztion> UserRightsInOrganiaztions { get; set; }
    }
}
