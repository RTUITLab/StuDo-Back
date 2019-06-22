using System;

namespace studo.Models
{
    public class UserRightsInOrganiaztion
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public Guid OrganizationRightId { get; set; }
        public OrganizationRight UserOrganizationRight { get; set; }
    }
}
