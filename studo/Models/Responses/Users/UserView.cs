using System;

namespace studo.Models.Responses.Users
{
    public class UserView
    {
        public Guid Id { get; set; }
        public string Firstname { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string StudentCardNumber { get; set; }
    }
}
