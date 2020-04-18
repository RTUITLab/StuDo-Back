using studo.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace studo.Services.Autorize
{
    public interface IJwtManager
    {
        string GenerateAccessToken(Guid userId, IEnumerable<string> roleNames);
        Task<string> SaveAndGetRefreshTokenAsync(User user);
        bool CheckTokenExpireTime(string token);
        Task<User> GetUserFromTokenAsync(string token);
        Task<RefreshToken> FindRefreshTokenAsync(string token);
        Task DeleteRefreshTokenAsync(RefreshToken refreshToken);
    }
}
