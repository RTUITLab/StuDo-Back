using studo.Models.Responses.Ads;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Models.Responses.Organization
{
    public class OrganizationView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<AdView> Ads { get; set; }
        //public List<UserRightsInOrganiaztionView> MyProperty { get; set; }
    }
}
