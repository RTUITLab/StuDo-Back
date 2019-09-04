using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Organization
{
    public class OrganizationCreateRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
