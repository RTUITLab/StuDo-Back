using System;

namespace studo.Models.Responses.Users
{
    public class CompactResumeView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public string UserName { get; set; }
        public Guid UserId { get; set; }
    }
}
