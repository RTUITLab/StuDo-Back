using System;

namespace studo.Models
{
    public class RefreshToken
    {
        public string Token { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }

        public DateTime ExpireTime { get; set; }
    }
}
