using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace studo.Models.Options
{
    public class JwtOptions
    {
        public string SecretKey { get; set; }
        public string Audience { get; set; }
        public string Issuer { get; set; }
        public TimeSpan LifeTime { get; set; }
        public SigningCredentials SigningCredentials =>
            new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey)), SecurityAlgorithms.HmacSha256);
        public SymmetricSecurityKey SymmetricSecurityKey =>
            new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));
    }
}
