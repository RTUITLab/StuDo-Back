using AutoMapper;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;
using System.Linq;

namespace studo.AutoMapperProfiles
{
    public class AdProfile : Profile
    {
        public AdProfile()
        {
            CreateMap<AdCreateRequest, Ad>();
            CreateMap<AdEditRequest, Ad>();
            CreateMap<Ad, AdView>()
                .ForMember(av => av.Comments,
                    map => map.MapFrom(a => a.Comments.OrderBy(com => com.CommentTime)));
            CreateMap<Ad, CompactAdView>()
                .ForMember(cav => cav.UserName,
                    map => map.MapFrom(a => a.User.Firstname + " " + a.User.Surname))
                .ForMember(cav => cav.OrganizationName,
                    map => map.MapFrom(a => a.Organization.Name))
                .ForMember(cav => cav.LastComment,
                    map => map.MapFrom(a => a.Comments.OrderByDescending(com => com.CommentTime).FirstOrDefault()));
        }
    }
}
