using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Ads
{
    public class AdEditRequest
    {
        [Required]
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
