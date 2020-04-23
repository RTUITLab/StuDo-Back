using AutoMapper;
using studo.Models;
using studo.Models.Requests.Users;
using studo.Models.Responses.Users;

namespace studo.AutoMapperProfiles
{
    public class ResumeProfile : Profile
    {
        public ResumeProfile()
        {
            CreateMap<CreateResumeRequest, Resume>();
            CreateMap<EditResumeRequest, Resume>();
            CreateMap<Resume, ResumeView>();
            CreateMap<Resume, CompactResumeView>()
                .ForMember(crv => crv.UserName, map => map.MapFrom(r => r.User.Firstname + " " + r.User.Surname));
        }
    }
}
