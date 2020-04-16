using Microsoft.Extensions.Options;
using studo.Models.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace studo.Services.Autorize
{
    public class JwtFactory : IJwtFactory
    {
        private readonly JwtOptions options;

        public JwtFactory(IOptions<JwtOptions> options)
        {
            this.options = options.Value;
        }
        public string GenerateAccessToken(Guid userId, IEnumerable<string> roleNames)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow + options.AccessTokenLifeTime).ToString(), ClaimValueTypes.Integer64)
            };
            claims.AddRange(roleNames.Select(name => new Claim(ClaimTypes.Role, name)));

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims,
                expires: DateTime.UtcNow + options.AccessTokenLifeTime,
                signingCredentials: options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        public string GenerateRefreshToken(Guid userId)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow + options.RefreshTokenLifeTime).ToString(), ClaimValueTypes.Integer64)
            };

            // Create the JWT security token and encode it.
            var jwt = new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                claims: claims,
                expires: DateTime.UtcNow + options.RefreshTokenLifeTime,
                signingCredentials: options.SigningCredentials);

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
            return encodedJwt;
        }

        private static long ToUnixEpochDate(DateTime date)
          => (long)Math.Round((date.ToUniversalTime() -
                               new DateTimeOffset(1970, 1, 1, 0, 0, 0, TimeSpan.Zero))
                              .TotalSeconds);
    }
}
