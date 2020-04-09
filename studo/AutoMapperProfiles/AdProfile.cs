using AutoMapper;
using studo.Models;
using studo.Models.Helpers;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;
using System;
using System.Linq;

namespace studo.AutoMapperProfiles
{
    public class AdProfile : Profile
    {
        public AdProfile()
        {
            CreateMap<AdCreateRequest, Ad>();
            CreateMap<AdEditRequest, Ad>();

            Guid currentUserId = default;
            CreateMap<Ad, AdAndUserId>()
                .ForMember(adui => adui.CurrentUserId, map => map.MapFrom(ad => currentUserId));

            CreateMap<AdAndUserId, AdView>()
                .ForMember(av => av.Comments,
                    map => map.MapFrom(a => a.Comments.OrderBy(com => com.CommentTime)))
                .ForMember(av => av.IsFavorite,
                    map => map.MapFrom(adui => adui.Bookmarks.Any(
                        ab => ab.AdId == adui.Id && ab.UserId == adui.CurrentUserId)));
            CreateMap<AdAndUserId, CompactAdView>()
                .ForMember(cav => cav.UserName,
                    map => map.MapFrom(a => a.User.Firstname + " " + a.User.Surname))
                .ForMember(cav => cav.OrganizationName,
                    map => map.MapFrom(a => a.Organization.Name))
                .ForMember(cav => cav.LastComment,
                    map => map.MapFrom(a => a.Comments.OrderByDescending(com => com.CommentTime).FirstOrDefault()))
                .ForMember(cav => cav.IsFavorite,
                    map => map.MapFrom(adui => adui.Bookmarks.Any(
                        ab => ab.AdId == adui.Id && ab.UserId == adui.CurrentUserId)));
        }
    }
}
