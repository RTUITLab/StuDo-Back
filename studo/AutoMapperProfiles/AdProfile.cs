using AutoMapper;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;

namespace studo.AutoMapperProfiles
{
    public class AdProfile : Profile
    {
        public AdProfile()
        {
            CreateMap<AdCreateRequest, Ad>();
            CreateMap<AdEditRequest, Ad>();
            CreateMap<Ad, CompactAdView>()
                .ForMember(cav => cav.UserName, map => map.MapFrom(a => a.User.Firstname + a.User.Surname))
                .ForMember(cav => cav.OrganizationName, map => map.MapFrom(a => a.Organization.Name));
            CreateMap<Ad, AdView>();
        }
    }
}
