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
            CreateMap<Ad, CompactAdView>();
            CreateMap<Ad, AdView>();
        }
    }
}
