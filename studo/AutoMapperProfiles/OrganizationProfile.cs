using AutoMapper;
using studo.Models;
using studo.Models.Requests.Organization;
using studo.Models.Responses.Organization;
using studo.Services.Configure;
using System.Linq;

namespace studo.AutoMapperProfiles
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Organization, OrganizationView>()
                .ForMember(ov => ov.CreatorId, map => map.MapFrom(org =>
                    org.Users
                    .FirstOrDefault(u => u.UserOrganizationRight.RightName == OrganizationRights.CanDeleteOrganization.ToString() && u.OrganizationId == org.Id)
                    .UserId))
                .ForMember(ov => ov.Creator, map => map.MapFrom(org =>
                    org.Users
                    .FirstOrDefault(u => u.UserOrganizationRight.RightName == OrganizationRights.CanDeleteOrganization.ToString() && u.OrganizationId == org.Id)
                    .User));
            CreateMap<OrganizationCreateRequest, Organization>();
            CreateMap<OrganizationEditRequest, Organization>();

            // members
            CreateMap<IGrouping<User, UserRightsInOrganization>, MemberView>()
                .ForMember(mv => mv.User, map => map.MapFrom(
                    ig => ig.Key))
                .ForMember(mv => mv.OrganizationRights, map => map.MapFrom(
                    ig => ig));

            CreateMap<UserRightsInOrganization, OrganizationRightView>()
                .ForMember(orv => orv.RightName, map => map.MapFrom(
                    urio => urio.UserOrganizationRight.RightName));
        }
    }
}
