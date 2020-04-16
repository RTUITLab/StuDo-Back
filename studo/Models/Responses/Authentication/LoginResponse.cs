using studo.Models.Responses.Users;

namespace studo.Models.Responses.Authentication
{
    public class LoginResponse
    {
        public UserView User { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
