using System;

namespace studo.Models.Responses.Ads
{
    public class CompactAdView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }

        public DateTime BeginTime { get; set; }
        public DateTime EndTime { get; set; }

        public Guid? UserId { get; set; }
        public string UserName { get; set; }

        public Guid? OrganizationId { get; set; }
        public string OrganizationName { get; set; }

        public CompactCommentView LastComment { get; set; }
    }
}
