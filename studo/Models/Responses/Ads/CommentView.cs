using System;

namespace studo.Models.Responses.Ads
{
    public class CommentView
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public DateTime CommentTime { get; set; }

        public Guid AuthorId { get; set; }
        public string Author { get; set; }
    }
}
