using studo.Models.Responses.Organization;
using studo.Models.Responses.Users;
using System;

namespace studo.Models.Responses.Ads
{
    public class AdView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }

        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public Guid? UserId { get; set; }
        public UserView User { get; set; }
        public Guid? OrganizationId { get; set; }
        public OrganizationView Organization { get; set; }
    }
}
