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
                    org.Users.Where(
                        u => u.UserOrganizationRight.RightName == OrganizationRights.CanDeleteOrganization.ToString() && u.OrganizationId == org.Id)
                            .SingleOrDefault().UserId))
                .ForMember(ov => ov.Creator, map => map.MapFrom(org =>
                    org.Users.Where(
                        u => u.UserOrganizationRight.RightName == OrganizationRights.CanDeleteOrganization.ToString() && u.OrganizationId == org.Id)
                        .SingleOrDefault().User));
            CreateMap<OrganizationCreateRequest, Organization>();
            CreateMap<OrganizationEditRequest, Organization>();
        }
    }
}
