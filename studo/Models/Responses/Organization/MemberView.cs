using studo.Models.Responses.Users;
using System.Collections.Generic;

namespace studo.Models.Responses.Organization
{
    public class MemberView
    {
        public UserView User { get; set; }
        public List<OrganizationRightView> OrganizationRights { get; set; }
    }
}
