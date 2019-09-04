using System;
using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Ads
{
    public class AdCreateRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
        [Required]
        public string ShortDescription { get; set; }

        [Required]
        public DateTime BeginTime { get; set; }
        [Required]
        public DateTime EndTime { get; set; }

        public Guid? OrganizationId { get; set; }
    }
}
