using AutoMapper;
using studo.Models;
using studo.Models.Requests.Ads;
using studo.Models.Responses.Ads;

namespace studo.AutoMapperProfiles
{
    public class CommentProfile : Profile
    {
        public CommentProfile()
        {
            CreateMap<AdCommentRequest, Comment>();
            CreateMap<Comment, CommentView>()
                .ForMember(cv => cv.Author, map => map.MapFrom(c => c.Author.Firstname + " " + c.Author.Surname));
            CreateMap<Comment, CompactCommentView>()
                .ForMember(ccv => ccv.Text, map => map.MapFrom(c => c.Text.Substring(0, 10).Trim()))
                .ForMember(ccv => ccv.Author, map => map.MapFrom(c => c.Author.Firstname + " " + c.Author.Surname));
        }
    }
}
