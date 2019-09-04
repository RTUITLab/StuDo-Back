using System;

namespace studo.Models.Responses.Users
{
    public class ResumeView
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid UserId { get; set; }
        public UserView User { get; set; }
    }
}
