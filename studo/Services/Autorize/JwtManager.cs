using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using studo.Data;
using studo.Models;
using studo.Models.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace studo.Services.Autorize
{
    public class JwtManager : IJwtManager
    {
        private readonly JwtOptions options;
        private readonly UserManager<User> userManager;
        private readonly DatabaseContext context;

        public JwtManager(IOptions<JwtOptions> options, UserManager<User> userManager,
            DatabaseContext context)
        {
            this.options = options.Value;
            this.userManager = userManager;
            this.context = context;
        }
        public string GenerateAccessToken(Guid userId, IEnumerable<string> roleNames)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, ToUnixEpochDate(DateTime.UtcNow + options.AccessTokenLifeTime).ToString(), ClaimValueTypes.Integer64)
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

        public async Task<string> SaveAndGetRefreshTokenAsync(User user)
        {
            var token = GenerateRefreshToken(user.Id);
            var refreshToken = new RefreshToken
            {
                User = user,
                UserId = user.Id,
                Token = token,
                ExpireTime = DateTime.UtcNow + options.RefreshTokenLifeTime
            };
            context.RefreshTokens.Add(refreshToken);
            await context.SaveChangesAsync();
            return token;
        }

        public bool CheckTokenExpireTime(string token)
        {
            var decodedJwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            return decodedJwt.ValidTo > DateTime.UtcNow;
        }

        public async Task<User> GetUserFromTokenAsync(string token)
        {
            var decodedJwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
            var claim = decodedJwt.Claims.ToList().FirstOrDefault(cl => cl.Type == ClaimTypes.NameIdentifier);
            return await userManager.FindByIdAsync(claim.Value);
        }

        public async Task<RefreshToken> FindRefreshTokenAsync(string token)
            => await context.RefreshTokens
                    .Where(rt => rt.Token == token)
                    .SingleOrDefaultAsync();

        public async Task DeleteRefreshTokenAsync(RefreshToken refreshToken)
        {
            context.RefreshTokens.Remove(refreshToken);
            await context.SaveChangesAsync();
        }

        private string GenerateRefreshToken(Guid userId)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, ToUnixEpochDate(DateTime.UtcNow).ToString(), ClaimValueTypes.Integer64),
                new Claim(JwtRegisteredClaimNames.Exp, ToUnixEpochDate(DateTime.UtcNow + options.RefreshTokenLifeTime).ToString(), ClaimValueTypes.Integer64)
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
