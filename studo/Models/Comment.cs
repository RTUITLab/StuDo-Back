using System;

namespace studo.Models
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime CommentTime { get; set; }

        public Guid AuthorId { get; set; }
        public User Author { get; set; }

        public Guid AdId { get; set; }
        public Ad Ad { get; set; }
    }
}
