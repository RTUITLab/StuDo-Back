using System;

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

        public Guid CreatorId { get; set; }
        public CreatorType CreatorType { get; set; }
    }
}
