using System.ComponentModel.DataAnnotations;

namespace studo.Models.Requests.Users
{
    public class CreateResumeRequest
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Description { get; set; }
    }
}
