using studo.Models.Responses.Users;
using System;

namespace studo.Models.Responses.Organization
{
    public class OrganizationView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public Guid CreatorId { get; set; }
        public UserView Creator { get; set; }
    }
}
