using System;
using System.Collections.Generic;

namespace studo.Models
{
    public class Ad
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public string Description { get; set; }

        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public Guid? UserId { get; set; }
        public User User { get; set; }
        public Guid? OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public List<UserAd> Bookmarks { get; set; }
        public List<Comment> Comments { get; set; }
    }
}
