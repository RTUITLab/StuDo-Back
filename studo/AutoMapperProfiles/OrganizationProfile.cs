using AutoMapper;
using studo.Models;
using studo.Models.Requests.Organization;
using studo.Models.Responses.Organization;

namespace studo.AutoMapperProfiles
{
    public class OrganizationProfile : Profile
    {
        public OrganizationProfile()
        {
            CreateMap<Organization, OrganizationView>();
            CreateMap<OrganizationCreateRequest, Organization>();
            CreateMap<OrganizationEditRequest, Organization>();
        }
    }
}
