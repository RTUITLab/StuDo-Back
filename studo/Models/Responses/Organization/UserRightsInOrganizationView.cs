using studo.Models.Responses.Users;

namespace studo.Models.Responses.Organization
{
    public class UserRightsInOrganizationView
    {
        public UserView User { get; set; }
        public OrganizationRightView OrganizationRight { get; set; }
        public OrganizationView Organization { get; set; }
    }
}
