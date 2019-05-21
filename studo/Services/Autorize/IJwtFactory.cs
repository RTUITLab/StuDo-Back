using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace studo.Services.Autorize
{
    public interface IJwtFactory
    {
        string GenerateAccessToken(Guid userId, IEnumerable<string> roleNames);
        string GenerateRefreshToken(Guid userId);
    }
}
