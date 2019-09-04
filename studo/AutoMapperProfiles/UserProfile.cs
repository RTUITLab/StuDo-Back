using AutoMapper;
using studo.Models;
using studo.Models.Requests.Authentication;
using studo.Models.Requests.Users;
using studo.Models.Responses.Users;

namespace studo.AutoMapperProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegistrationRequest, User>();
            CreateMap<User, UserView>();
            CreateMap<ChangeUserInformationRequest, User>();
        }
    }
}
