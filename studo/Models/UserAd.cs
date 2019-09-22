using System;

namespace studo.Models
{
    public class UserAd
    {
        public Guid UserId { get; set; }
        public User User { get; set; }

        public Guid AdId { get; set; }
        public Ad Ad { get; set; }
    }
}
